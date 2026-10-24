using System.Collections.Concurrent;
using System.Text;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.TemporaryExports;
using Xunit;
namespace Ropabajo.Church.Sanluis.Objects.Api.Tests;

public class TemporaryExportTests
{
    [Fact]
    public async Task Repeated_requests_and_two_replicas_generate_one_file_and_reuse_for_five_minutes()
    {
        var f = new Fixture(); var initial = await f.Reserve();
        Assert.True(initial.NeedsSource);
        for (var i = 0; i < 10; i++)
        {
            var reused = await f.Reserve(); Assert.Equal(initial.ExportId, reused.ExportId);
            Assert.True(reused.Reused); Assert.False(reused.NeedsSource);
        }
        await f.Service.SupplyAsync(f.Scope, initial.ExportId, f.Source(), default);
        var otherReplica = f.NewCoordinator();
        await Task.WhenAll(f.Service.ReconcileAsync(null, default), otherReplica.ReconcileAsync(null, default));
        Assert.Equal(1, f.Writer.Calls); Assert.Single(f.Store.Data.Keys, k => k.EndsWith("document.xlsx"));
        var ready = await f.Service.StatusAsync(f.Scope, initial.ExportId, default);
        Assert.Equal("ready", ready.Status); Assert.Equal(f.Clock.Now.AddHours(24), ready.ExpiresAt);
        f.Clock.Advance(TimeSpan.FromSeconds(299)); Assert.Equal(initial.ExportId, (await f.Reserve()).ExportId);
        f.Clock.Advance(TimeSpan.FromSeconds(2)); Assert.NotEqual(initial.ExportId, (await f.Reserve()).ExportId);
    }
    [Fact]
    public async Task A_second_replica_cannot_render_while_the_first_is_uploading_the_same_job()
    {
        var f = new Fixture(); var job = await f.Reserve();
        await f.Service.SupplyAsync(f.Scope, job.ExportId, f.Source(), default);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Store.BeforeWrite = async key => { if (key.EndsWith("document.xlsx")) { entered.SetResult(); await release.Task; } };
        var first = f.Service.ReconcileAsync(null, default);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await f.NewCoordinator().ReconcileAsync(null, default);
        Assert.Equal(1, f.Writer.Calls);
        release.SetResult(); await first;
        Assert.Equal("ready", (await f.Service.StatusAsync(f.Scope, job.ExportId, default)).Status);
    }
    [Fact]
    public async Task A_missing_ready_file_can_be_exported_again_without_reusing_a_broken_download()
    {
        var f = new Fixture(); var job = await f.Ready();
        var key = f.Store.Data.Keys.Single(k => k.EndsWith("document.xlsx")); f.Store.Data.TryRemove(key, out _);
        Assert.NotEqual(job.ExportId, (await f.Reserve()).ExportId);
    }
    [Fact]
    public async Task Idempotency_key_survives_retries_but_cannot_be_reused_for_different_filters()
    {
        var f = new Fixture(); var id = Guid.NewGuid(); var job = await f.Reserve(id);
        Assert.Equal(job.ExportId, (await f.Reserve(id)).ExportId);
        var error = await Assert.ThrowsAsync<ExportFlowException>(() => f.Reserve(id, fingerprint: new string('b', 64)));
        Assert.Equal("invalid", error.Code);
    }
    [Fact]
    public async Task Refresh_creates_new_data_only_after_ready_and_never_duplicates_pending_work()
    {
        var f = new Fixture(); var job = await f.Reserve();
        await f.Service.SupplyAsync(f.Scope, job.ExportId, f.Source(), default);
        f.Clock.Advance(TimeSpan.FromMinutes(6));
        Assert.Equal(job.ExportId, (await f.Reserve(force: true)).ExportId);
        await f.Service.ReconcileAsync(null, default);
        Assert.NotEqual(job.ExportId, (await f.Reserve(force: true)).ExportId);
    }
    [Fact]
    public async Task Download_renewal_uses_same_object_and_is_bounded_by_file_expiry()
    {
        var f = new Fixture(); var job = await f.Ready(); var count = f.Writer.Calls;
        var first = await f.Service.DownloadAsync(f.Scope, job.ExportId, default);
        Assert.Equal(f.Clock.Now.AddMinutes(15), first.ExpiresAt);
        f.Clock.Advance(TimeSpan.FromMinutes(16));
        var second = await f.Service.DownloadAsync(f.Scope, job.ExportId, default);
        Assert.NotEqual(first.ExpiresAt, second.ExpiresAt); Assert.Equal(count, f.Writer.Calls);
        f.Clock.Advance(TimeSpan.FromHours(24) - TimeSpan.FromMinutes(16) - TimeSpan.FromSeconds(9));
        var last = await f.Service.DownloadAsync(f.Scope, job.ExportId, default);
        Assert.True(last.ExpiresAt <= job.ExpiresAt); Assert.True(f.Store.LastSignedSeconds <= 9);
        f.Clock.Advance(TimeSpan.FromSeconds(10));
        Assert.Equal("expired", (await Assert.ThrowsAsync<ExportFlowException>(() => f.Service.DownloadAsync(f.Scope, job.ExportId, default))).Code);
        Assert.Contains(f.Store.Data.Keys, k => k.EndsWith("document.xlsx")); // Logical expiry does not wait for physical cleanup.
    }
    [Fact]
    public async Task Cleanup_removes_only_temporary_expired_objects_and_preserves_photos_and_election_pdfs()
    {
        var f = new Fixture(); var job = await f.Ready();
        f.Store.Data["photos/permanent.jpg"] = [1]; f.Store.Data["reports/elections/permanent.pdf"] = [2];
        f.Clock.Advance(TimeSpan.FromHours(24)); await f.Service.ReconcileAsync(null, default);
        Assert.DoesNotContain(f.Store.Data.Keys, k => k.StartsWith(TemporaryExportCoordinator.Prefix));
        Assert.Equal(2, f.Store.Data.Count);
        Assert.Equal("missing", (await Assert.ThrowsAsync<ExportFlowException>(() => f.Service.StatusAsync(f.Scope, job.ExportId, default))).Code);
    }
    [Fact]
    public async Task Cleanup_does_not_delete_an_index_replaced_by_a_concurrent_reservation()
    {
        var f = new Fixture(); await f.Ready(); f.Clock.Advance(TimeSpan.FromHours(24));
        var active = f.Store.Data.Keys.Single(k => k.Contains("/active-"));
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reads = 0;
        f.Store.AfterRead = async key =>
        {
            if (key == active && Interlocked.Increment(ref reads) == 1) { entered.SetResult(); await release.Task; }
        };
        var cleanup = f.Service.ReconcileAsync(null, default);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var next = await f.Reserve();
        release.SetResult(); await cleanup;
        Assert.Equal(next.ExportId, (await f.Reserve()).ExportId);
    }
    [Fact]
    public async Task Another_user_or_church_cannot_read_supply_or_download_an_export()
    {
        var f = new Fixture(); var job = await f.Ready();
        foreach (var scope in new[] { f.Scope with { Owner = new string('f', 64) }, f.Scope with { ChurchCode = Guid.NewGuid() } })
        {
            Assert.Equal("missing", (await Assert.ThrowsAsync<ExportFlowException>(() => f.Service.StatusAsync(scope, job.ExportId, default))).Code);
            Assert.Equal("missing", (await Assert.ThrowsAsync<ExportFlowException>(() => f.Service.DownloadAsync(scope, job.ExportId, default))).Code);
            Assert.Equal("missing", (await Assert.ThrowsAsync<ExportFlowException>(() => f.Service.SupplyAsync(scope, job.ExportId, f.Source(), default))).Code);
        }
    }
    [Fact]
    public async Task Restart_recovers_durable_source_and_repeated_supply_never_replaces_the_original()
    {
        var f = new Fixture(); var job = await f.Reserve();
        await f.Service.SupplyAsync(f.Scope, job.ExportId, f.Source("Original"), default);
        await f.Service.SupplyAsync(f.Scope, job.ExportId, f.Source("Replacement"), default);
        await f.NewCoordinator().ReconcileAsync(null, default);
        Assert.Equal("Original", f.Writer.LastSource!.Title);
        Assert.DoesNotContain(f.Store.Data.Keys, k => k.EndsWith("source.json"));
    }
    [Fact]
    public async Task Incomplete_input_times_out_without_creating_a_workbook_and_can_be_requested_again()
    {
        var f = new Fixture(); var job = await f.Reserve(); f.Clock.Advance(TimeSpan.FromSeconds(121));
        Assert.Equal("failed", (await f.Service.StatusAsync(f.Scope, job.ExportId, default)).Status);
        await f.Service.ReconcileAsync(null, default);
        Assert.Equal(0, f.Writer.Calls); Assert.NotEqual(job.ExportId, (await f.Reserve()).ExportId);
    }
    [Fact]
    public async Task Invalid_or_oversized_sources_are_rejected_without_silent_truncation()
    {
        var f = new Fixture(); var job = await f.Reserve();
        foreach (var source in new[] { f.Source() with { Columns = [new("Unknown", "formula")] },
            f.Source() with { Rows = Enumerable.Range(0, 10001).Select(_ => (IReadOnlyList<string?>)new string?[] { "x" }).ToArray() },
            f.Source() with { Rows = [new string?[] { "x", "extra" }] } })
            Assert.Equal("invalid", (await Assert.ThrowsAsync<ExportFlowException>(() => f.Service.SupplyAsync(f.Scope, job.ExportId, source, default))).Code);
        Assert.Equal(0, f.Writer.Calls); Assert.DoesNotContain(f.Store.Data.Keys, k => k.EndsWith("source.json"));
    }
    [Fact]
    public async Task A_poison_job_does_not_block_others_and_stops_after_three_attempts()
    {
        var f = new Fixture(); var bad = await f.Reserve();
        await f.Service.SupplyAsync(f.Scope, bad.ExportId, f.Source("bad"), default);
        var good = await f.Reserve(fingerprint: new string('c', 64));
        await f.Service.SupplyAsync(f.Scope, good.ExportId, f.Source("good"), default);
        f.Writer.FailTitle = "bad";
        for (var i = 0; i < 4; i++) await f.Service.ReconcileAsync(null, default);
        Assert.Equal("failed", (await f.Service.StatusAsync(f.Scope, bad.ExportId, default)).Status);
        Assert.Equal("ready", (await f.Service.StatusAsync(f.Scope, good.ExportId, default)).Status);
        Assert.Equal(4, f.Writer.Calls);
    }
    [Fact]
    public async Task Disabled_flow_does_not_scan_or_store_files()
    {
        var f = new Fixture(); f.Options.Enabled = false;
        Assert.Equal("disabled", (await Assert.ThrowsAsync<ExportFlowException>(() => f.Reserve())).Code);
        Assert.Null(await f.Service.ReconcileAsync(null, default)); Assert.Empty(f.Store.Data);
    }
    [Fact]
    public void Real_xlsx_preserves_all_rows_dates_accents_and_formula_like_names_as_text()
    {
        var now = DateTimeOffset.Parse("2026-10-24T12:00:00Z");
        var source = new ExportWorkbookSource("Iglesia de prueba", [new("Nombre"), new("Fecha", "date"), new("Número", "integer")],
            Enumerable.Range(1, 35).Select(n => (IReadOnlyList<string?>)new string?[] { n == 1 ? "=HYPERLINK(\"invalid\")" : "PÉREZ ÑÚÑEZ ANA", "2020-05-26", n.ToString() }).ToArray(), now);
        var bytes = new TemporaryWorkbookWriter().Write(source, now, default);
        using var stream = new MemoryStream(bytes); using var book = new XLWorkbook(stream); var sheet = book.Worksheet(1);
        Assert.Equal(39, sheet.LastRowUsed()!.RowNumber()); Assert.False(sheet.Cell(5, 1).HasFormula);
        Assert.Equal(new DateTime(2020, 5, 26), sheet.Cell(5, 2).GetDateTime()); Assert.Equal(35, sheet.Cell(39, 3).GetDouble());
        Assert.Contains("Datos consultados:", sheet.Cell(2, 1).GetString());
    }
}

internal sealed class Fixture
{
    public readonly MemoryObjects Store = new(); public readonly MemoryLeases Leases = new(); public readonly CountingWriter Writer = new();
    public readonly ExportClock Clock = new(); public readonly TemporaryExportOptions Options = new() { Enabled = true };
    public readonly ExportScope Scope = new(Guid.NewGuid(), new string('a', 64));
    public TemporaryExportCoordinator Service { get; }
    public Fixture() { Service = NewCoordinator(); }
    public TemporaryExportCoordinator NewCoordinator() => new(Store, Leases, Writer, Microsoft.Extensions.Options.Options.Create(Options), Clock);
    public ExportWorkbookSource Source(string title = "Prueba") => new(title, [new("Nombre")], [new string?[] { "ANA PÉREZ" }], Clock.Now);
    public Task<TemporaryExportResponse> Reserve(Guid? requestId = null, bool force = false, string? fingerprint = null)
        => Service.ReserveAsync(Scope, new(requestId ?? Guid.NewGuid(), fingerprint ?? new string('d', 64), "members-v1", force), default);
    public async Task<TemporaryExportResponse> Ready() { var job = await Reserve(); await Service.SupplyAsync(Scope, job.ExportId, Source(), default); await Service.ReconcileAsync(null, default); return await Service.StatusAsync(Scope, job.ExportId, default); }
}
internal sealed class ExportClock : TimeProvider
{
    public DateTimeOffset Now = DateTimeOffset.Parse("2026-10-24T12:00:00Z");
    public override DateTimeOffset GetUtcNow() => Now;
    public void Advance(TimeSpan value) => Now += value;
}
internal sealed class MemoryObjects : IExportObjectStore
{
    public readonly ConcurrentDictionary<string, byte[]> Data = new(); public int LastSignedSeconds;
    public Func<string, Task>? BeforeWrite;
    public Func<string, Task>? AfterRead;
    public async Task<byte[]?> ReadAsync(string key, CancellationToken token)
    {
        Data.TryGetValue(key, out var bytes);
        if (AfterRead is not null) await AfterRead(key);
        return bytes;
    }
    public async Task WriteAsync(string key, byte[] bytes, string contentType, CancellationToken token) { if (BeforeWrite is not null) await BeforeWrite(key); Data[key] = bytes; }
    public Task<bool> ExistsAsync(string key, CancellationToken token) => Task.FromResult(Data.ContainsKey(key));
    public Task<IReadOnlyList<string>> ListAsync(string prefix, CancellationToken token) => Task.FromResult<IReadOnlyList<string>>(Data.Keys.Where(k => k.StartsWith(prefix)).ToArray());
    public Task DeleteAsync(string key, CancellationToken token) { Data.TryRemove(key, out _); return Task.CompletedTask; }
    public Task<string> SignAsync(string key, string filename, int seconds, DateTimeOffset issuedAt, CancellationToken token) { LastSignedSeconds = seconds; return Task.FromResult("https://minio.example.invalid/" + key + "?expires=" + seconds); }
}
internal sealed class MemoryLeases : IExportLeaseFactory
{
    private readonly ConcurrentDictionary<string, byte> active = new();
    public Task<IAsyncDisposable?> TryAcquireAsync(string key, CancellationToken token) => Task.FromResult<IAsyncDisposable?>(active.TryAdd(key, 1) ? new Lease(() => active.TryRemove(key, out _)) : null);
    private sealed class Lease(Action release) : IAsyncDisposable { public ValueTask DisposeAsync() { release(); return ValueTask.CompletedTask; } }
}
internal sealed class CountingWriter : ITemporaryWorkbookWriter
{
    public int Calls; public string? FailTitle; public ExportWorkbookSource? LastSource;
    public byte[] Write(ExportWorkbookSource source, DateTimeOffset generatedAt, CancellationToken token)
    { Interlocked.Increment(ref Calls); LastSource = source; if (source.Title == FailTitle) throw new IOException("Fixture renderer interruption"); return Encoding.ASCII.GetBytes("xlsx-fixture"); }
}
