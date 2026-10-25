using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Ropabajo.Churc.Sanluis.Framework.Core;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Api.Controllers;
using System.Text.Json;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Persistence;
using Xunit;

public sealed class FileJobLifecycleTests : IAsyncLifetime
{
    private readonly Clock _clock = new();
    private readonly Artifacts _files = new();
    private Factory _factory = null!;
    private SqlFileJobStore _store = null!;
    private FileJobService _service = null!;
    private readonly FileJobRequester _owner = new(Guid.NewGuid(), new string('a', 64), new[] { "MEMBER_VIEW" });
    private readonly FileJobsSettings _settings = new() { Enabled = true };
    public async Task InitializeAsync()
    {
        // Only an explicitly supplied disposable test database is permitted.
        var connection = Environment.GetEnvironmentVariable("FILE_JOBS_TEST_MYSQL") ?? throw new InvalidOperationException("Run with the documented disposable MySQL fixture.");
        var sql = new MySqlConnector.MySqlConnectionStringBuilder(connection);
        if (sql.Server != "127.0.0.1" || sql.Port != 13316) throw new InvalidOperationException("The fixture must be local and isolated.");
        sql.Database = "file_jobs_test_" + Guid.NewGuid().ToString("N");
        _factory = new Factory(new DbContextOptionsBuilder<FileJobsDbContext>().UseMySql(sql.ConnectionString, new MySqlServerVersion(new Version(5, 7, 44))).Options);
        await using var context = _factory.CreateDbContext(); await context.Database.EnsureCreatedAsync();
        await File.WriteAllTextAsync("/tmp/file-jobs-schema.sql", context.Database.GenerateCreateScript());
        _store = new SqlFileJobStore(_factory);
        var mapper = new MapperConfiguration(config => config.AddProfile<FileJobProjectionMappingProfile>(), NullLoggerFactory.Instance).CreateMapper();
        _service = new FileJobService(_store, _files, mapper, Options.Create(_settings), _clock);
    }
    public async Task DisposeAsync() { await using var context = _factory.CreateDbContext(); await context.Database.EnsureDeletedAsync(); }
    private Task<Result<FileJobSummary>> Create(Guid? intent = null, object? parameters = null, bool force = false)
        => _service.CreateAsync(_owner, intent ?? Guid.NewGuid(), "members.excel", 1, JsonSerializer.SerializeToElement(parameters ?? new { fullName = "PÉREZ", stateCode = (int?)null }), force, default);
    private async Task<FileJobWorkItem> Claim(Guid job) => (await _service.ClaimAsync("church", job, default)).Value;
    private async Task<FileJobSummary> Ready()
    {
        var job = (await Create()).Value; var work = await Claim(job.JobId);
        var grant = (await _service.UploadAsync("church", job.JobId, work.AttemptId, work.LeaseToken, new FileUploadRequest { Name = "workbook", SizeBytes = 1200, Sha256 = new string('b', 64) }, default)).Value;
        return (await _service.CompleteAsync("church", job.JobId, work.AttemptId, work.LeaseToken, new FileCompletion { ArtifactIds = new[] { grant.ArtifactId }, DataAsOf = _clock.GetUtcNow() }, default)).Value;
    }

    [Fact]
    public void Reviewed_schema_matches_the_model_and_selects_only_the_isolated_database()
    {
        var migration = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../operations/file-jobs/001_metadata_dev.sql"));
        using var context = _factory.CreateDbContext(); var script = File.ReadAllText(migration).Replace("\r\n", "\n");
        Assert.Contains("USE `db_file_jobs_dev`;", script);
        Assert.EndsWith(context.Database.GenerateCreateScript().Replace("\r\n", "\n").TrimEnd(), script.TrimEnd());
        Assert.DoesNotContain("USE `db_object`", script);
    }

    [Fact]
    public async Task Concurrent_intentions_share_one_job_and_transactional_outbox()
    {
        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Create()));
        Assert.All(results, value => Assert.True(value.IsSuccess)); Assert.Single(results.Select(value => value.Value.JobId).Distinct());
        await using var context = _factory.CreateDbContext(); Assert.Equal(1, await context.Jobs.CountAsync()); Assert.Equal(1, await context.Outbox.CountAsync()); Assert.Equal(8, await context.Intents.CountAsync());
        Assert.True((await Create(force: true)).Value.Reused);
    }

    [Fact]
    public async Task Same_intention_is_stable_but_cannot_change_its_parameters()
    {
        var intent = Guid.NewGuid(); var first = (await Create(intent)).Value;
        Assert.Equal(first.JobId, (await Create(intent)).Value.JobId);
        Assert.Equal("FILE_JOB_INTENT_CONFLICT", (await Create(intent, new { fullName = "OTRA" })).Errors.Single().Code);
        Assert.True((await Create(parameters: new { rows = new[] { "private" } })).IsFailure);
    }

    [Fact]
    public async Task Unconfirmed_publication_remains_pending_and_only_dispatch_owner_can_confirm()
    {
        await Create(); var id = Assert.Single(await _store.PendingMessagesAsync("development", _clock.Utc, 20, default));
        Assert.Empty(await _store.PendingMessagesAsync("production", _clock.Utc, 20, default));
        var dispatcher = Guid.NewGuid(); Assert.NotNull(await _service.ClaimDispatchAsync(id, dispatcher, default));
        Assert.Null(await _service.ClaimDispatchAsync(id, Guid.NewGuid(), default));
        Assert.False(await _service.ConfirmDispatchAsync(id, Guid.NewGuid(), default));
        await _service.RetryDispatchAsync(id, dispatcher, default);
        await using (var context = _factory.CreateDbContext()) Assert.Null((await context.Outbox.SingleAsync()).PublishedAt);
        _clock.Advance(20); Assert.NotNull(await _service.ClaimDispatchAsync(id, dispatcher, default));
        Assert.True(await _service.ConfirmDispatchAsync(id, dispatcher, default));
        Assert.Empty(await _store.PendingMessagesAsync("development", _clock.Utc, 20, default));
    }

    [Fact]
    public async Task Lost_worker_is_retried_and_late_completion_cannot_publish()
    {
        var id = (await Create()).Value.JobId; var first = await Claim(id);
        _clock.Advance(121); await _service.ReconcileAsync(id, default);
        Assert.Equal("retry_wait", (await _store.FindAsync(id, default))!.Status);
        _clock.Advance(40); var second = await Claim(id); Assert.NotEqual(first.AttemptId, second.AttemptId);
        var stale = await _service.CompleteAsync("church", id, first.AttemptId, first.LeaseToken, new FileCompletion(), default);
        Assert.Equal("FILE_JOB_ATTEMPT_STALE", stale.Errors.Single().Code);
        Assert.True((await _service.FailAsync("church", id, second.AttemptId, second.LeaseToken, new FileFailure { Code = "unsafe exception body", Retryable = true }, default)).IsSuccess);
        Assert.Equal("FILE_GENERATION_FAILED", (await _store.FindAsync(id, default))!.FailureCode);
    }

    [Fact]
    public async Task Ready_requires_integrity_and_lost_completion_response_is_idempotent()
    {
        var id = (await Create()).Value.JobId; var work = await Claim(id);
        var grant = (await _service.UploadAsync("church", id, work.AttemptId, work.LeaseToken, new FileUploadRequest { Name = "workbook", SizeBytes = 1200, Sha256 = new string('b', 64) }, default)).Value;
        var completion = new FileCompletion { ArtifactIds = new[] { grant.ArtifactId }, DataAsOf = _clock.GetUtcNow() };
        _files.Valid = false;
        Assert.True((await _service.CompleteAsync("church", id, work.AttemptId, work.LeaseToken, completion, default)).IsFailure);
        Assert.Equal("running", (await _store.FindAsync(id, default))!.Status);
        _files.Valid = true;
        Assert.Equal("ready", (await _service.CompleteAsync("church", id, work.AttemptId, work.LeaseToken, completion, default)).Value.Status);
        Assert.Equal("ready", (await _service.CompleteAsync("church", id, work.AttemptId, work.LeaseToken, completion, default)).Value.Status);
        Assert.Equal("{}", (await _store.FindAsync(id, default))!.ParametersJson);
    }

    [Fact]
    public async Task Download_reuses_artifact_caps_signature_and_logical_expiry_precedes_cleanup()
    {
        var ready = await Ready(); var artifact = Assert.Single(ready.Artifacts);
        var first = (await _service.DownloadAsync(_owner, ready.JobId, artifact.ArtifactId, default)).Value;
        Assert.Equal(_clock.GetUtcNow().AddMinutes(15), first.ExpiresAt);
        _clock.Advance(16 * 60); Assert.True((await _service.DownloadAsync(_owner, ready.JobId, artifact.ArtifactId, default)).IsSuccess);
        _clock.Advance(24 * 3600 - 16 * 60 - 8);
        Assert.Equal(ready.ExpiresAt, (await _service.DownloadAsync(_owner, ready.JobId, artifact.ArtifactId, default)).Value.ExpiresAt);
        _clock.Advance(9); Assert.True((await _service.DownloadAsync(_owner, ready.JobId, artifact.ArtifactId, default)).IsFailure);
        Assert.Equal("expired", (await _service.GetAsync(_owner, ready.JobId, default)).Value.Status);
        await _service.ReconcileAsync(ready.JobId, default); Assert.Equal(1, _files.Deleted);
        await _service.ReconcileAsync(ready.JobId, default); Assert.Equal(1, _files.Deleted);
        _clock.Advance(31 * 86400); await _store.PurgeAsync("development", _clock.Utc.AddDays(-30), 25, default);
        Assert.Null(await _store.FindAsync(ready.JobId, default));
    }

    [Fact]
    public async Task Attempt_timeouts_retry_within_the_total_deadline_and_stop_after_the_third_attempt()
    {
        var id = (await Create()).Value.JobId;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            await Claim(id); _clock.Advance(901); await _service.ReconcileAsync(id, default);
            var stored = (await _store.FindAsync(id, default))!;
            Assert.Equal(attempt == 3 ? "failed" : "retry_wait", stored.Status);
            _clock.Advance(attempt == 1 ? 40 : 130);
        }
        Assert.True((await _service.ClaimAsync("church", id, default)).IsFailure);
    }

    [Fact]
    public async Task Missing_artifact_is_not_reused_as_a_broken_ready_file()
    {
        var ready = await Ready(); _files.Missing = true;
        Assert.True((await _service.DownloadAsync(_owner, ready.JobId, ready.Artifacts[0].ArtifactId, default)).IsFailure);
        Assert.Equal("failed", (await _service.GetAsync(_owner, ready.JobId, default)).Value.Status);
        Assert.NotEqual(ready.JobId, (await Create()).Value.JobId);
    }

    [Fact]
    public async Task Ownership_roles_church_and_environment_are_enforced_and_pagination_is_stable()
    {
        var first = (await Create()).Value;
        Assert.True((await _service.GetAsync(_owner with { OwnerHash = new string('b', 64) }, first.JobId, default)).IsFailure);
        Assert.True((await _service.GetAsync(_owner with { ChurchCode = Guid.NewGuid() }, first.JobId, default)).IsFailure);
        Assert.True((await _service.GetAsync(_owner with { Roles = Array.Empty<string>() }, first.JobId, default)).IsFailure);
        Assert.True((await _service.ClaimAsync("other", first.JobId, default)).IsFailure);
        await Create(parameters: new { fullName = "DOS" }); await Create(parameters: new { fullName = "TRES" });
        var page = (await _service.ListAsync(_owner, null, 2, default)).Value; Assert.Equal(2, page.Items.Count); Assert.NotNull(page.NextCursor);
        var last = (await _service.ListAsync(_owner, page.NextCursor, 2, default)).Value; Assert.Single(last.Items); Assert.Equal(first.JobId, last.Items[0].JobId);
        Assert.True((await Create(parameters: new { fullName = "CUATRO" })).IsFailure);
    }

    [Fact]
    public async Task Cancel_and_attempt_exhaustion_are_durable_and_bounded()
    {
        var id = (await Create()).Value.JobId; var work = await Claim(id);
        Assert.Equal("cancel_requested", (await _service.CancelAsync(_owner, id, default)).Value.Status);
        Assert.Equal("FILE_JOB_TERMINAL", (await _service.HeartbeatAsync("church", id, work.AttemptId, work.LeaseToken, default)).Errors.Single().Code);
        Assert.Equal("canceled", (await _store.FindAsync(id, default))!.Status);
        id = (await Create()).Value.JobId;
        for (var i = 0; i < 3; i++) { work = await Claim(id); await _service.FailAsync("church", id, work.AttemptId, work.LeaseToken, new FileFailure { Retryable = true }, default); _clock.Advance(130); }
        Assert.Equal("failed", (await _store.FindAsync(id, default))!.Status); Assert.True((await _service.ClaimAsync("church", id, default)).IsFailure);
    }

    [Fact]
    public async Task Actual_controller_pipeline_preserves_202_maps_validation_and_scopes_list_and_cancel()
    {
        var builder = WebApplication.CreateBuilder(); builder.WebHost.UseTestServer();
        builder.Services.AddAuthentication(); builder.Services.AddAuthorization(options => options.AddPolicy("ActiveChurchRoute", policy => policy.RequireAssertion(_ => true)));
        var actor = new Actor(_owner); builder.Services.AddSingleton<IFileJobActor>(actor); builder.Services.AddSingleton(_service);
        var assembly = typeof(FileJobService).Assembly;
        builder.Services.AddAutoMapper(config => {
            config.AddProfile<FileJobProjectionMappingProfile>();
            foreach (var type in assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Profile)) && type.Namespace?.Contains(".Features.FileJobs.") == true)) config.AddProfile(type);
        });
        builder.Services.AddMediator(assembly); builder.Services.AddScoped<FileJobExceptionFilter>();
        builder.Services.AddControllers(options => options.Filters.Add(typeof(ResultFilter))).AddApplicationPart(typeof(FileJobController).Assembly);
        await using var app = builder.Build(); app.UseAuthorization(); app.MapControllers(); await app.StartAsync(); var client = app.GetTestClient();
        var path = $"/v1/church/{_owner.ChurchCode:D}/file-jobs";
        var requestId = Guid.NewGuid();
        var accepted = await client.PostAsJsonAsync(path, new { requestId, reportType = "members.excel", contractVersion = 1, parameters = new { } });
        Assert.Equal(HttpStatusCode.Accepted, accepted.StatusCode);
        using var body = JsonDocument.Parse(await accepted.Content.ReadAsStringAsync()); Assert.True(body.RootElement.GetProperty("isSuccess").GetBoolean());
        var id = body.RootElement.GetProperty("value").GetProperty("jobId").GetGuid();
        var page = await client.GetAsync(path + "?pageSize=10"); Assert.Equal(HttpStatusCode.OK, page.StatusCode); Assert.Contains(id.ToString(), await page.Content.ReadAsStringAsync());
        var invalid = await client.PostAsJsonAsync(path, new { requestId = Guid.Empty, reportType = "members.excel", parameters = new { } });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode); Assert.Equal("application/problem+json", invalid.Content.Headers.ContentType!.MediaType);
        Assert.Contains("REQUEST_ID_REQUIRED", await invalid.Content.ReadAsStringAsync());
        actor.Owner = _owner with { Roles = Array.Empty<string>() };
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync(path, new { requestId = Guid.NewGuid(), reportType = "members.excel", parameters = new { } })).StatusCode);
        actor.Owner = _owner;
        var canceled = await client.PostAsJsonAsync(path + $"/{id:D}/cancel", new { }); Assert.Equal(HttpStatusCode.OK, canceled.StatusCode); Assert.Contains("canceled", await canceled.Content.ReadAsStringAsync());
    }

    private sealed class Actor(FileJobRequester owner) : IFileJobActor
    {
        public FileJobRequester Owner = owner;
        public FileJobRequester GetRequester(Guid churchCode) => churchCode == Owner.ChurchCode ? Owner : throw new UnauthorizedAccessException();
        public string GetProducer() => "church";
    }

    private sealed class Clock : TimeProvider { public DateTime Utc = new(2026, 10, 25, 15, 0, 0, DateTimeKind.Utc); public override DateTimeOffset GetUtcNow() => new(Utc); public void Advance(int seconds) => Utc = Utc.AddSeconds(seconds); }
    private sealed class Factory(DbContextOptions<FileJobsDbContext> options) : IDbContextFactory<FileJobsDbContext> { public FileJobsDbContext CreateDbContext() => new(options); }
    private sealed class Artifacts : IFileArtifactStore
    {
        public bool Valid = true, Missing; public int Deleted;
        public Task<FileUploadGrant> GrantAsync(FileJobArtifact artifact, DateTimeOffset expires, CancellationToken ct) => Task.FromResult(new FileUploadGrant { ArtifactId = artifact.ArtifactId, ExpiresAt = expires, Url = "https://minio.example.org" });
        public Task<bool> VerifyAsync(FileJobArtifact artifact, CancellationToken ct) => Task.FromResult(Valid);
        public Task<Result<FileDownload>> SignAsync(FileJobArtifact artifact, DateTimeOffset issued, DateTimeOffset expires, CancellationToken ct) => Task.FromResult(Missing ? Result.Failure<FileDownload>(new Error("FILE_ARTIFACT_MISSING", "Archivo ausente.", ErrorType.NotFound)) : Result<FileDownload>.Success(new FileDownload { Url = "https://minio.example.org/file", ExpiresAt = expires, FileName = artifact.FileName }));
        public Task DeleteAsync(FileJobArtifact artifact, CancellationToken ct) { Deleted++; return Task.CompletedTask; }
    }
}
