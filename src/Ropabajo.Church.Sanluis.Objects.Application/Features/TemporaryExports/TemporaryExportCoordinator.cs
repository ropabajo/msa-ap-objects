using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;

/// <summary>Coordina reutilización, idempotencia, vigencia y publicación de XLSX privados.</summary>
public sealed class TemporaryExportCoordinator(IExportObjectStore store, IExportLeaseFactory leases,
    ITemporaryWorkbookWriter writer, IOptions<TemporaryExportOptions> settings, TimeProvider clock, ILogger<TemporaryExportCoordinator>? logger = null)
{
    /// <summary>Único prefijo administrado por el flujo y su limpieza.</summary>
    public const string Prefix = "temporary-exports/v1/";
    private TemporaryExportOptions Options => settings.Value;
    private DateTimeOffset Now => clock.GetUtcNow();
    private static string ScopeKey(ExportScope scope) => $"{scope.ChurchCode:D}/{scope.Owner}";
    private static string JobRoot(ExportScope scope, Guid id) => $"{Prefix}jobs/{ScopeKey(scope)}/{id:D}/";
    private static string Manifest(ExportScope scope, Guid id) => JobRoot(scope, id) + "manifest.json";
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static string ActiveKey(ExportScope scope, string fingerprint) => $"{Prefix}indexes/{ScopeKey(scope)}/active-{fingerprint}.json";
    private static string RequestKey(ExportScope scope, Guid id) => $"{Prefix}indexes/{ScopeKey(scope)}/request-{id:D}.json";
    private void Enabled() { if (!Options.Enabled) throw new ExportFlowException("disabled", "La exportación temporal no está habilitada en este ambiente."); }
    private static void ValidScope(ExportScope scope)
    {
        if (scope.ChurchCode == Guid.Empty || !Regex.IsMatch(scope.Owner ?? "", "^[a-f0-9]{64}$"))
            throw new ExportFlowException("missing", "No se encontró la exportación para esta sesión e iglesia.");
    }
    private async Task<T?> Read<T>(string key, CancellationToken token) where T : class
    {
        var bytes = await store.ReadAsync(key, token);
        return bytes is null ? null : JsonSerializer.Deserialize<T>(bytes) ?? throw new InvalidDataException("Invalid export metadata.");
    }
    private Task Save<T>(string key, T value, CancellationToken token) => store.WriteAsync(key, JsonSerializer.SerializeToUtf8Bytes(value), "application/json", token);
    private bool Live(ExportJob job) => job.ExpiresAt > Now;
    private bool Reusable(ExportJob job) => Live(job) && ((job.Status == "pending")
        || (job.Status == "ready" && job.GeneratedAt!.Value.AddSeconds(Options.ReuseSeconds) > Now));
    private async Task<ExportJob?> Indexed(string key, string fingerprint, ExportScope scope, bool exactRequest, CancellationToken token)
    {
        var index = await Read<ExportIndex>(key, token);
        if (index is null || index.ExpiresAt <= Now) return null;
        if (index.Fingerprint != fingerprint)
        {
            if (exactRequest) throw new ExportFlowException("invalid", "La solicitud ya se utilizó para una búsqueda diferente.");
            return null;
        }
        if (!index.JobKey.StartsWith(Prefix + "jobs/" + ScopeKey(scope) + "/", StringComparison.Ordinal)) throw new InvalidDataException("Invalid export index scope.");
        var job = await Read<ExportJob>(index.JobKey, token);
        return job is not null && job.Scope == scope && Live(job) ? job : null;
    }
    private static TemporaryExportResponse Response(ExportJob job, bool reused = false, bool needsSource = false)
        => new(job.Id, job.Status, job.CreatedAt, job.GeneratedAt, job.ExpiresAt, job.DataAsOf, job.FileName, reused, needsSource);

    /// <summary>Diez clics o peticiones simultáneas comparten un trabajo dentro del mismo ámbito.</summary>
    public async Task<TemporaryExportResponse> ReserveAsync(ExportScope scope, ReserveExportRequest request, CancellationToken token)
    {
        Enabled(); ValidScope(scope);
        if (request.RequestId == Guid.Empty || request.Fingerprint is null || !Regex.IsMatch(request.Fingerprint, "^[a-f0-9]{64}$")
            || request.Kind is null || !Regex.IsMatch(request.Kind, "^[a-z][a-z0-9-]{0,39}$"))
            throw new ExportFlowException("invalid", "La solicitud de exportación no es válida.");
        var fingerprint = Hash(request.Kind + ":" + request.Fingerprint);
        await using var requestLease = await leases.TryAcquireAsync("request:" + ScopeKey(scope) + ":" + request.RequestId, token)
            ?? throw new ExportFlowException("busy", "La solicitud está siendo reservada. Vuelve a consultar con el mismo identificador.");
        await using var lease = await leases.TryAcquireAsync("reserve:" + ScopeKey(scope) + ":" + fingerprint, token)
            ?? throw new ExportFlowException("busy", "La exportación se está reservando. Vuelve a consultar con la misma solicitud.");
        var requestKey = RequestKey(scope, request.RequestId);
        var original = await Indexed(requestKey, fingerprint, scope, true, token);
        if (original is not null) return Response(original, true, original.Status == "pending"
            && original.RequestId == request.RequestId && !await store.ExistsAsync(JobRoot(scope, original.Id) + "source.json", token));
        var activeKey = ActiveKey(scope, fingerprint);
        var active = await Indexed(activeKey, fingerprint, scope, false, token);
        if (active?.Status == "ready" && !await store.ExistsAsync(JobRoot(scope, active.Id) + "document.xlsx", token)) active = null;
        if (active is not null && Reusable(active) && (active.Status == "pending" || !request.ForceRefresh))
        {
            await Save(requestKey, new ExportIndex(Manifest(scope, active.Id), fingerprint, active.ExpiresAt), token);
            var needsSource = active.Status == "pending" && active.RequestId == request.RequestId
                && !await store.ExistsAsync(JobRoot(scope, active.Id) + "source.json", token);
            return Response(active, true, needsSource);
        }
        var now = Now; var id = Guid.NewGuid();
        var job = new ExportJob(id, scope, request.RequestId, fingerprint, request.Kind, "pending", now,
            now.AddSeconds(Options.InputTimeoutSeconds), null, now.AddHours(Options.RetentionHours), null,
            $"{request.Kind}-{now:yyyyMMdd-HHmmss}-{id.ToString("N")[..8]}.xlsx");
        var manifest = Manifest(scope, id);
        await Save(manifest, job, token);
        var reference = new ExportIndex(manifest, fingerprint, job.ExpiresAt);
        await Save(activeKey, reference, token); await Save(requestKey, reference, token);
        return Response(job, false, true);
    }

    /// <summary>Recibe una sola fuente durable; reenvíos no reemplazan datos ni generan más libros.</summary>
    public async Task<TemporaryExportResponse> SupplyAsync(ExportScope scope, Guid id, ExportWorkbookSource source, CancellationToken token)
    {
        Enabled(); ValidScope(scope);
        await using var lease = await leases.TryAcquireAsync("job:" + ScopeKey(scope) + ":" + id, token)
            ?? throw new ExportFlowException("busy", "La exportación está siendo procesada. Vuelve a consultar su estado.");
        var job = await GetJob(scope, id, token);
        if (job.Status != "pending" || await store.ExistsAsync(JobRoot(scope, id) + "source.json", token)) return Response(job, true);
        if (job.SourceDeadline <= Now) throw new ExportFlowException("expired", "Terminó el plazo de preparación. Solicita nuevamente la exportación.");
        ValidateSource(source);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(source);
        if (bytes.Length > Options.MaxSourceBytes) throw new ExportFlowException("invalid", "La exportación supera el tamaño permitido. Ajusta los filtros.");
        await store.WriteAsync(JobRoot(scope, id) + "source.json", bytes, "application/json", token);
        return Response(job);
    }
    private void ValidateSource(ExportWorkbookSource source)
    {
        if (string.IsNullOrWhiteSpace(source.Title) || source.Title.Length > 300 || source.Columns is null || source.Rows is null
            || source.Columns.Count < 1 || source.Columns.Count > Options.MaxColumns || source.Rows.Count > Options.MaxRows
            || source.DataAsOf == default || source.DataAsOf > Now.AddMinutes(1)
            || source.Columns.Any(c => c is null || string.IsNullOrWhiteSpace(c.Title) || c.Title.Length > 100
                || c.Kind is not ("text" or "date" or "integer") || c.Width < 5 || c.Width > 80))
            throw new ExportFlowException("invalid", "Los datos de la exportación no son válidos o exceden los límites.");
        foreach (var row in source.Rows)
        {
            if (row is null || row.Count != source.Columns.Count) throw new ExportFlowException("invalid", "Una fila no coincide con las columnas del reporte.");
            for (var i = 0; i < row.Count; i++)
            {
                var value = row[i];
                if (value is null) continue;
                if (value.Length > 1000 || (source.Columns[i].Kind == "date" && !DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    || (source.Columns[i].Kind == "integer" && !long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)))
                    throw new ExportFlowException("invalid", "Una celda tiene un valor incompatible con su columna.");
            }
        }
    }
    private async Task<ExportJob> GetJob(ExportScope scope, Guid id, CancellationToken token)
    {
        var job = await Read<ExportJob>(Manifest(scope, id), token);
        if (job is null || job.Id != id || job.Scope != scope) throw new ExportFlowException("missing", "No se encontró la exportación para esta sesión e iglesia.");
        if (!Live(job)) throw new ExportFlowException("expired", "La exportación venció. Solicita una nueva con los datos actuales.");
        return job;
    }
    /// <summary>Consulta el mismo archivo sin renovar datos ni iniciar generación.</summary>
    public async Task<TemporaryExportResponse> StatusAsync(ExportScope scope, Guid id, CancellationToken token)
    {
        Enabled(); ValidScope(scope); var job = await GetJob(scope, id, token);
        if (job.Status == "pending" && job.SourceDeadline <= Now && !await store.ExistsAsync(JobRoot(scope, id) + "source.json", token))
            return Response(job with { Status = "failed" });
        return Response(job);
    }
    /// <summary>Renueva únicamente la firma; nunca prolonga las 24 horas del archivo.</summary>
    public async Task<ExportDownloadResponse> DownloadAsync(ExportScope scope, Guid id, CancellationToken token)
    {
        Enabled(); ValidScope(scope); var job = await GetJob(scope, id, token);
        if (job.Status != "ready") throw new ExportFlowException("pending", "El archivo todavía no está disponible para descargar.");
        var key = JobRoot(scope, id) + "document.xlsx";
        if (!await store.ExistsAsync(key, token)) throw new ExportFlowException("missing", "El archivo ya no está disponible. Solicita otra exportación.");
        var issuedAt = Now;
        var seconds = Math.Min(Options.LinkSeconds, (int)Math.Floor((job.ExpiresAt - issuedAt).TotalSeconds));
        if (seconds < 1) throw new ExportFlowException("expired", "La exportación venció. Solicita una nueva.");
        return new(await store.SignAsync(key, job.FileName, seconds, issuedAt, token), issuedAt.AddSeconds(seconds));
    }

    /// <summary>Recupera trabajos tras reinicios y elimina solo objetos del prefijo temporal vencidos.</summary>
    public async Task<string?> ReconcileAsync(string? afterKey, CancellationToken token)
    {
        if (!Options.Enabled) return null;
        var all = await store.ListAsync(Prefix + "jobs/", token);
        var batch = all.Where(k => k.EndsWith("/manifest.json", StringComparison.Ordinal)
            && (afterKey is null || string.CompareOrdinal(k, afterKey) > 0)).OrderBy(k => k, StringComparer.Ordinal).Take(Options.BatchSize).ToArray();
        foreach (var key in batch)
        {
            Guid? exportId = null;
            try
            {
                token.ThrowIfCancellationRequested();
                var initial = await Read<ExportJob>(key, token);
                if (initial is null || key != Manifest(initial.Scope, initial.Id)) continue;
                exportId = initial.Id;
                if (Live(initial) && initial.Status != "pending") continue;
                await using var lease = await leases.TryAcquireAsync("job:" + ScopeKey(initial.Scope) + ":" + initial.Id, token);
                if (lease is null) continue;
                var job = await Read<ExportJob>(key, token); if (job is null) continue;
                var root = JobRoot(job.Scope, job.Id);
                if (!Live(job))
                {
                    await store.DeleteAsync(root + "document.xlsx", token); await store.DeleteAsync(root + "source.json", token);
                    await store.DeleteAsync(key, token); continue;
                }
                if (job.Status != "pending") continue;
                var source = await Read<ExportWorkbookSource>(root + "source.json", token);
                if (source is null)
                {
                    if (job.SourceDeadline <= Now) await Save(key, job with { Status = "failed" }, token);
                    continue;
                }
                if (job.Attempts >= Options.MaxAttempts) { await Save(key, job with { Status = "failed" }, token); continue; }
                job = job with { Attempts = job.Attempts + 1 }; await Save(key, job, token);
                ValidateSource(source);
                var generatedAt = Now; var bytes = writer.Write(source, generatedAt, token);
                await store.WriteAsync(root + "document.xlsx", bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", token);
                await Save(key, job with
                {
                    Status = "ready",
                    GeneratedAt = generatedAt,
                    ExpiresAt = generatedAt.AddHours(Options.RetentionHours),
                    DataAsOf = source.DataAsOf
                }, token);
                await store.DeleteAsync(root + "source.json", token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
            catch (Exception exception) { logger?.LogWarning("Temporary export pending recovery. ExportId={ExportId} FailureType={FailureType}", exportId, exception.GetType().Name); }
        }
        if (batch.Length < Options.BatchSize)
        {
            foreach (var key in await store.ListAsync(Prefix + "indexes/", token))
            {
                if (!key.EndsWith(".json", StringComparison.Ordinal)) continue;
                try
                {
                    await CleanupIndexAsync(key, token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
                catch (Exception exception) { logger?.LogWarning("Temporary export index cleanup deferred. FailureType={FailureType}", exception.GetType().Name); }
            }
            return null;
        }
        return batch[^1];
    }

    private async Task CleanupIndexAsync(string key, CancellationToken token)
    {
        var index = await Read<ExportIndex>(key, token);
        if (index is null || index.ExpiresAt > Now) return;
        var parts = key[(Prefix + "indexes/").Length..].Split('/');
        if (parts.Length != 3 || !Guid.TryParseExact(parts[0], "D", out var church)
            || !Regex.IsMatch(parts[1], "^[a-f0-9]{64}$")) return;
        var scope = new ExportScope(church, parts[1]);
        string leaseKey;
        if (parts[2].StartsWith("active-", StringComparison.Ordinal)
            && Regex.IsMatch(index.Fingerprint, "^[a-f0-9]{64}$") && key == ActiveKey(scope, index.Fingerprint))
            leaseKey = "reserve:" + ScopeKey(scope) + ":" + index.Fingerprint;
        else if (parts[2].StartsWith("request-", StringComparison.Ordinal) && parts[2].EndsWith(".json", StringComparison.Ordinal)
            && Guid.TryParseExact(parts[2][8..^5], "D", out var request) && key == RequestKey(scope, request))
            leaseKey = "request:" + ScopeKey(scope) + ":" + request;
        else return;
        await using var lease = await leases.TryAcquireAsync(leaseKey, token);
        if (lease is null) return;
        // A new reservation may have replaced an expired index after the first read.
        var current = await Read<ExportIndex>(key, token);
        if (current is not null && current.ExpiresAt <= Now) await store.DeleteAsync(key, token);
    }
}
