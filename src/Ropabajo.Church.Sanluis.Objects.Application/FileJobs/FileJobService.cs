using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.Files.Messaging;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs;

/// <summary>
/// Aplica el ciclo de vida durable de trabajos y separa transacciones de operaciones externas.
/// </summary>
public sealed partial class FileJobService
{
    private const string WorkbookMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private readonly IFileJobStore _store;
    private readonly IFileArtifactStore _files;
    private readonly IMapper _mapper;
    private readonly FileJobsSettings _settings;
    private readonly TimeProvider _clock;
    private DateTime Now => _clock.GetUtcNow().UtcDateTime;

    /// <summary>
    /// Inicializa puertos, reloj y límites del ambiente.
    /// </summary>
    public FileJobService(IFileJobStore store, IFileArtifactStore files, IMapper mapper,
        IOptions<FileJobsSettings> settings, TimeProvider clock)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _files = files ?? throw new ArgumentNullException(nameof(files));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _settings = (settings ?? throw new ArgumentNullException(nameof(settings))).Value;
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Registra o reutiliza un trabajo y su outbox dentro de una transacción local.
    /// </summary>
    public Task<Result<FileJobSummary>> CreateAsync(FileJobRequester requester, Guid requestId, string reportType,
        int contractVersion, JsonElement parameters, bool forceRefresh, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled) return Task.FromResult(Result.Failure<FileJobSummary>(FileJobErrors.Disabled));
        if (!ValidRequester(requester) || requestId == Guid.Empty || reportType != "members.excel" || contractVersion != 1
            || parameters.ValueKind != JsonValueKind.Object)
            return Task.FromResult(Result.Failure<FileJobSummary>(FileJobErrors.Invalid));
        if (!CanExport(requester)) return Task.FromResult(Result.Failure<FileJobSummary>(FileJobErrors.Forbidden));
        string normalized;
        try { normalized = MemberParameters(parameters); }
        catch (Exception exception) when (exception is JsonException or InvalidDataException)
        { return Task.FromResult(Result.Failure<FileJobSummary>(FileJobErrors.Invalid)); }
        var scope = Hash($"{_settings.Environment}:{requester.ChurchCode:D}:{requester.OwnerHash}");
        var fingerprint = Hash($"{scope}:{reportType}:{contractVersion}:members-xlsx-1:{normalized}");
        return _store.WriteAsync<Result<FileJobSummary>>(async (session, token) =>
        {
            var intent = await session.IntentAsync(scope, requestId, token);
            if (intent is not null)
            {
                if (intent.Fingerprint != fingerprint) return Result.Failure<FileJobSummary>(FileJobErrors.IntentConflict);
                var original = await session.JobAsync(intent.JobId, token);
                return original is null ? Result.Failure<FileJobSummary>(FileJobErrors.NotFound) : Summary(original, true);
            }
            var slot = await session.SlotAsync(fingerprint, token);
            var active = slot is null ? null : await session.JobAsync(slot.JobId, token);
            var now = Now;
            var reusable = active is not null && (Active(active.Status) || (active.Status == "ready"
                && (active.ExpiresAt is null || active.ExpiresAt > now) && active.DataAsOf?.AddSeconds(active.ReuseSeconds) > now && !forceRefresh));
            FileJob job;
            if (reusable) job = active!;
            else
            {
                var activeCount = await session.ActiveCountAsync(_settings.Environment, requester.ChurchCode, requester.OwnerHash, token);
                if (activeCount >= _settings.MaxActivePerOwner)
                    return Result.Failure<FileJobSummary>(FileJobErrors.Quota);
                job = new FileJob
                {
                    JobId = Guid.NewGuid(), Environment = _settings.Environment, ChurchCode = requester.ChurchCode,
                    OwnerHash = requester.OwnerHash, ReportType = reportType, ContractVersion = contractVersion,
                    Producer = "church", Fingerprint = fingerprint, ParametersJson = normalized, CreatedAt = now, UpdatedAt = now,
                    DeadlineAt = now.AddSeconds(_settings.JobDeadlineSeconds), RetentionHours = _settings.RetentionHours, ReuseSeconds = _settings.ReuseSeconds
                };
                session.AddJob(job);
                if (slot is null) session.AddSlot(new FileJobSlot { Fingerprint = fingerprint, JobId = job.JobId });
                else { slot.JobId = job.JobId; slot.Version++; }
                Enqueue(session, job, requestId, now);
                Audit(session, job, "FILE_JOB_CREATED");
            }
            session.AddIntent(new FileJobIntent { Scope = scope, RequestId = requestId, Fingerprint = fingerprint, JobId = job.JobId, CreatedAt = now });
            return Summary(job, reusable);
        }, cancellationToken);
    }

    /// <summary>
    /// Consulta un trabajo del usuario y su iglesia sin regenerar el archivo.
    /// </summary>
    public async Task<Result<FileJobSummary>> GetAsync(FileJobRequester requester, Guid jobId, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled) return Result.Failure<FileJobSummary>(FileJobErrors.Disabled);
        var job = await _store.FindAsync(jobId, cancellationToken);
        if (!Owns(job, requester)) return Result.Failure<FileJobSummary>(FileJobErrors.NotFound);
        var result = Summary(job!);
        if (job!.Status == "ready" && job.ExpiresAt <= Now) { result.Status = "expired"; result.Artifacts = Array.Empty<FileArtifactSummary>(); }
        return result;
    }

    /// <summary>
    /// Lista trabajos autorizados mediante un cursor de secuencia, sin parámetros del reporte.
    /// </summary>
    public async Task<Result<FileJobPage>> ListAsync(FileJobRequester requester, string? cursor, int pageSize, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled) return Result.Failure<FileJobPage>(FileJobErrors.Disabled);
        if (!ValidRequester(requester) || pageSize < 1 || pageSize > 50) return Result.Failure<FileJobPage>(FileJobErrors.Invalid);
        if (!CanExport(requester)) return Result.Failure<FileJobPage>(FileJobErrors.Forbidden);
        long? before = null;
        if (cursor is not null)
        {
            try
            {
                if (cursor.Length > 128 || !long.TryParse(Encoding.UTF8.GetString(Convert.FromBase64String(cursor)), NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value < 1)
                    return Result.Failure<FileJobPage>(FileJobErrors.Invalid);
                before = value;
            }
            catch (FormatException) { return Result.Failure<FileJobPage>(FileJobErrors.Invalid); }
        }
        var jobs = await _store.ListAsync(_settings.Environment, requester.ChurchCode, requester.OwnerHash, before, pageSize + 1, cancellationToken);
        var selected = jobs.Take(pageSize).ToArray();
        var items = selected.Select(job => { var item = Summary(job); if (job.Status == "ready" && job.ExpiresAt <= Now) { item.Status = "expired"; item.Artifacts = Array.Empty<FileArtifactSummary>(); } return item; }).ToList();
        return new FileJobPage { Items = items, NextCursor = jobs.Count > pageSize ? Convert.ToBase64String(Encoding.UTF8.GetBytes(selected[^1].Sequence.ToString(CultureInfo.InvariantCulture))) : null };
    }

    /// <summary>
    /// Otorga un intento exclusivo al generador técnico correcto.
    /// </summary>
    public Task<Result<FileJobWorkItem>> ClaimAsync(string producer, Guid jobId, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled) return Task.FromResult(Result.Failure<FileJobWorkItem>(FileJobErrors.Disabled));
        return _store.WriteAsync<Result<FileJobWorkItem>>(async (session, token) =>
        {
            var job = await session.JobAsync(jobId, token);
            if (!GeneratorOwns(job, producer)) return Result.Failure<FileJobWorkItem>(FileJobErrors.NotFound);
            var now = Now;
            if (job!.Status == "cancel_requested") { Terminal(session, job, "canceled", "FILE_JOB_CANCELED"); return Result.Failure<FileJobWorkItem>(FileJobErrors.Terminal); }
            if (!Active(job.Status)) return Result.Failure<FileJobWorkItem>(FileJobErrors.Terminal);
            if (job.DeadlineAt <= now || job.AttemptCount >= _settings.MaxAttempts && !(job.Status == "running" && job.LeaseUntil > now))
            { Terminal(session, job, "failed", "FILE_JOB_TIMEOUT_OR_ATTEMPTS"); return Result.Failure<FileJobWorkItem>(FileJobErrors.Terminal); }
            if (job.Status == "running" && job.LeaseUntil > now || job.Status == "retry_wait" && job.NextAttemptAt > now)
                return Result.Failure<FileJobWorkItem>(FileJobErrors.Busy);
            var leaseToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
            job.AttemptId = Guid.NewGuid(); job.AttemptCount++; job.Status = "running"; job.LeaseHash = Hash(leaseToken);
            job.AttemptDeadlineAt = Min(job.DeadlineAt, now.AddSeconds(_settings.RunSeconds));
            job.LeaseUntil = Min(job.AttemptDeadlineAt.Value, now.AddSeconds(_settings.LeaseSeconds));
            job.NextAttemptAt = null; job.UpdatedAt = now; job.Version++;
            Audit(session, job, "FILE_JOB_CLAIMED");
            return new FileJobWorkItem { JobId = job.JobId, AttemptId = job.AttemptId.Value, LeaseToken = leaseToken,
                LeaseUntil = Utc(job.LeaseUntil.Value), DeadlineAt = Utc(job.AttemptDeadlineAt.Value), ChurchCode = job.ChurchCode,
                ReportType = job.ReportType, ContractVersion = job.ContractVersion, Parameters = JsonSerializer.Deserialize<JsonElement>(job.ParametersJson) };
        }, cancellationToken);
    }

    /// <summary>
    /// Renueva un intento vigente sin superar su plazo máximo.
    /// </summary>
    public Task<Result<FileJobSummary>> HeartbeatAsync(string producer, Guid jobId, Guid attemptId, string leaseToken, CancellationToken cancellationToken)
        => !_settings.Enabled ? Task.FromResult(Result.Failure<FileJobSummary>(FileJobErrors.Disabled)) : _store.WriteAsync<Result<FileJobSummary>>(async (session, token) =>
        {
            var job = await session.JobAsync(jobId, token);
            if (!ValidAttempt(job, producer, attemptId, leaseToken)) return Result.Failure<FileJobSummary>(FileJobErrors.Stale);
            if (job!.Status == "cancel_requested") { Terminal(session, job, "canceled", "FILE_JOB_CANCELED"); return Result.Failure<FileJobSummary>(FileJobErrors.Terminal); }
            job.LeaseUntil = Min(job.AttemptDeadlineAt!.Value, Now.AddSeconds(_settings.LeaseSeconds)); job.UpdatedAt = Now; job.Version++;
            return Summary(job);
        }, cancellationToken);

    /// <summary>
    /// Reserva un archivo del intento antes de solicitar una política de subida externa.
    /// </summary>
    public async Task<Result<FileUploadGrant>> UploadAsync(string producer, Guid jobId, Guid attemptId, string leaseToken,
        FileUploadRequest request, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled) return Result.Failure<FileUploadGrant>(FileJobErrors.Disabled);
        if (request.Name != "workbook" || request.SizeBytes < 1 || request.SizeBytes > _settings.MaxBytes || request.Sha256 is null || !Regex.IsMatch(request.Sha256, "^[a-f0-9]{64}$"))
            return Result.Failure<FileUploadGrant>(FileJobErrors.InvalidFiles);
        var reserved = await _store.WriteAsync<Result<FileJobArtifact>>(async (session, token) =>
        {
            var job = await session.JobAsync(jobId, token);
            if (!ValidAttempt(job, producer, attemptId, leaseToken) || job!.Status != "running") return Result.Failure<FileJobArtifact>(FileJobErrors.Stale);
            var existing = job.Artifacts.SingleOrDefault(value => value.AttemptId == attemptId && value.Name == request.Name);
            if (existing is not null)
                return existing.SizeBytes == request.SizeBytes && existing.Sha256 == request.Sha256 ? existing : Result.Failure<FileJobArtifact>(FileJobErrors.IntentConflict);
            var artifact = new FileJobArtifact
            {
                ArtifactId = Guid.NewGuid(), JobId = jobId, AttemptId = attemptId, Name = request.Name,
                FileName = $"miembros-{Now:yyyyMMdd}-{jobId:N}.xlsx", ContentType = WorkbookMime,
                SizeBytes = request.SizeBytes, Sha256 = request.Sha256, CreatedAt = Now,
                StorageKey = $"file-jobs/v1/{_settings.Environment}/{job.ChurchCode:D}/{jobId:D}/{attemptId:D}/workbook.xlsx"
            };
            job.Artifacts.Add(artifact); job.Version++; return artifact;
        }, cancellationToken);
        if (reserved.IsFailure) return Result.Failure<FileUploadGrant>(reserved.Errors.ToArray());
        return await _files.GrantAsync(reserved.Value, _clock.GetUtcNow().AddSeconds(_settings.UploadSeconds), cancellationToken);
    }

    /// <summary>
    /// Verifica los archivos sin mantener una transacción SQL durante la lectura de MinIO.
    /// </summary>
    public async Task<Result<FileJobSummary>> CompleteAsync(string producer, Guid jobId, Guid attemptId, string leaseToken,
        FileCompletion completion, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled) return Result.Failure<FileJobSummary>(FileJobErrors.Disabled);
        var snapshot = await _store.FindAsync(jobId, cancellationToken);
        if (GeneratorOwns(snapshot, producer) && snapshot!.Status == "ready" && snapshot.AttemptId == attemptId && SameLease(snapshot, leaseToken)) return Summary(snapshot);
        if (!ValidAttempt(snapshot, producer, attemptId, leaseToken) || snapshot!.Status != "running") return Result.Failure<FileJobSummary>(FileJobErrors.Stale);
        var artifacts = snapshot.Artifacts.Where(value => value.AttemptId == attemptId).ToArray();
        if (completion.DataAsOf == default || completion.DataAsOf > _clock.GetUtcNow().AddMinutes(1) || artifacts.Length != 1
            || completion.ArtifactIds.Count != 1 || artifacts[0].ArtifactId != completion.ArtifactIds[0])
            return Result.Failure<FileJobSummary>(FileJobErrors.InvalidFiles);
        foreach (var artifact in artifacts) if (!await _files.VerifyAsync(artifact, cancellationToken)) return Result.Failure<FileJobSummary>(FileJobErrors.InvalidFiles);
        return await _store.WriteAsync<Result<FileJobSummary>>(async (session, token) =>
        {
            var job = await session.JobAsync(jobId, token);
            if (GeneratorOwns(job, producer) && job!.Status == "ready" && job.AttemptId == attemptId && SameLease(job, leaseToken)) return Summary(job);
            if (!ValidAttempt(job, producer, attemptId, leaseToken) || job!.Status != "running") return Result.Failure<FileJobSummary>(FileJobErrors.Stale);
            foreach (var artifact in job.Artifacts.Where(value => value.AttemptId == attemptId)) artifact.Published = true;
            job.Status = "ready"; job.GeneratedAt = Now; job.DataAsOf = completion.DataAsOf.UtcDateTime;
            job.ExpiresAt = job.RetentionHours is null ? null : job.GeneratedAt.Value.AddHours(job.RetentionHours.Value);
            job.ParametersJson = "{}"; job.UpdatedAt = Now; job.LeaseUntil = null; job.Version++;
            Audit(session, job, "FILE_JOB_READY"); return Summary(job);
        }, cancellationToken);
    }

    /// <summary>
    /// Registra un fallo del intento y programa su reintento junto con una nueva outbox.
    /// </summary>
    public Task<Result<FileJobSummary>> FailAsync(string producer, Guid jobId, Guid attemptId, string leaseToken,
        FileFailure failure, CancellationToken cancellationToken)
        => !_settings.Enabled ? Task.FromResult(Result.Failure<FileJobSummary>(FileJobErrors.Disabled)) : _store.WriteAsync<Result<FileJobSummary>>(async (session, token) =>
        {
            var job = await session.JobAsync(jobId, token);
            if (!ValidAttempt(job, producer, attemptId, leaseToken)) return Result.Failure<FileJobSummary>(FileJobErrors.Stale);
            if (job!.Status == "cancel_requested") { Terminal(session, job, "canceled", "FILE_JOB_CANCELED"); return Summary(job); }
            // Provider messages never cross this boundary; only a fixed safe category is persisted.
            var code = failure.Code is "FILE_REPORT_INVALID_DATA" or "FILE_JOB_EXECUTION_TIMEOUT" ? failure.Code : "FILE_GENERATION_FAILED";
            if (!failure.Retryable || job.AttemptCount >= _settings.MaxAttempts || job.DeadlineAt <= Now) Terminal(session, job, "failed", code);
            else Retry(session, job, code);
            return Summary(job);
        }, cancellationToken);

    /// <summary>
    /// Solicita cancelación del trabajo sin cambiar un estado de negocio del módulo.
    /// </summary>
    public Task<Result<FileJobSummary>> CancelAsync(FileJobRequester requester, Guid jobId, CancellationToken cancellationToken)
        => !_settings.Enabled ? Task.FromResult(Result.Failure<FileJobSummary>(FileJobErrors.Disabled)) : _store.WriteAsync<Result<FileJobSummary>>(async (session, token) =>
        {
            var job = await session.JobAsync(jobId, token);
            if (!Owns(job, requester)) return Result.Failure<FileJobSummary>(FileJobErrors.NotFound);
            if (!Active(job!.Status)) return Result.Failure<FileJobSummary>(FileJobErrors.Terminal);
            if (job.Status == "running" && job.LeaseUntil > Now)
            { job.Status = "cancel_requested"; job.UpdatedAt = Now; job.Version++; Audit(session, job, "FILE_JOB_CANCEL_REQUESTED"); }
            else Terminal(session, job, "canceled", "FILE_JOB_CANCELED");
            return Summary(job);
        }, cancellationToken);

    /// <summary>
    /// Firma un archivo publicado del usuario sin regenerar ni ampliar la retención.
    /// </summary>
    public async Task<Result<FileDownload>> DownloadAsync(FileJobRequester requester, Guid jobId, Guid artifactId, CancellationToken cancellationToken)
    {
        if (!_settings.Enabled) return Result.Failure<FileDownload>(FileJobErrors.Disabled);
        var job = await _store.FindAsync(jobId, cancellationToken);
        if (!Owns(job, requester)) return Result.Failure<FileDownload>(FileJobErrors.NotFound);
        if (job!.Status != "ready" || job.ExpiresAt <= Now) return Result.Failure<FileDownload>(FileJobErrors.Unavailable);
        var artifact = job.Artifacts.SingleOrDefault(value => value.ArtifactId == artifactId && value.Published);
        if (artifact is null) return Result.Failure<FileDownload>(FileJobErrors.NotFound);
        var now = _clock.GetUtcNow(); var expires = now.AddSeconds(_settings.LinkSeconds);
        if (job.ExpiresAt is not null && Utc(job.ExpiresAt.Value) < expires) expires = Utc(job.ExpiresAt.Value);
        if ((expires - now).TotalSeconds < 1) return Result.Failure<FileDownload>(FileJobErrors.Unavailable);
        var signed = await _files.SignAsync(artifact, now, expires, cancellationToken);
        if (signed.IsFailure && signed.Errors.Any(error => error.Type == ErrorType.NotFound))
        {
            await _store.WriteAsync(async (session, token) =>
            {
                var current = await session.JobAsync(jobId, token);
                if (Owns(current, requester) && current!.Status == "ready") Terminal(session, current, "failed", "FILE_ARTIFACT_MISSING");
                return true;
            }, cancellationToken);
        }
        return signed;
    }

    /// <summary>
    /// Recupera un lease abandonado o vence un trabajo sin mantener acceso a archivos durante SQL.
    /// </summary>
    public async Task ReconcileAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var removable = await _store.WriteAsync(async (session, token) =>
        {
            var job = await session.JobAsync(jobId, token);
            if (job is null || job.Environment != _settings.Environment) return Array.Empty<FileJobArtifact>();
            var now = Now;
            if (job.Status == "ready" && job.ExpiresAt <= now) Terminal(session, job, "expired", "FILE_JOB_EXPIRED");
            else if (Active(job.Status) && job.DeadlineAt <= now) Terminal(session, job, "failed", "FILE_JOB_TIMEOUT");
            else if (job.Status == "cancel_requested" && job.LeaseUntil <= now) Terminal(session, job, "canceled", "FILE_JOB_CANCELED");
            else if (job.Status == "running" && job.AttemptDeadlineAt <= now)
            {
                if (job.AttemptCount >= _settings.MaxAttempts) Terminal(session, job, "failed", "FILE_JOB_EXECUTION_TIMEOUT");
                else Retry(session, job, "FILE_JOB_EXECUTION_TIMEOUT");
            }
            else if (job.Status == "running" && job.LeaseUntil <= now)
            {
                if (job.AttemptCount >= _settings.MaxAttempts) Terminal(session, job, "failed", "FILE_JOB_ATTEMPTS_EXHAUSTED");
                else Retry(session, job, "FILE_JOB_LEASE_EXPIRED");
            }
            // Wait for old upload grants to expire before removing abandoned attempt objects.
            return job.Artifacts.Where(value => value.DeletedAt == null && ((!value.Published && value.CreatedAt.AddHours(24) <= now && value.AttemptId != job.AttemptId)
                || job.Status == "expired" && value.CreatedAt.AddSeconds(_settings.UploadSeconds + 60) <= now
                || job.Status is "failed" or "canceled" && value.CreatedAt.AddHours(24) <= now)).ToArray();
        }, cancellationToken);
        foreach (var artifact in removable)
        {
            await _files.DeleteAsync(artifact, cancellationToken);
            await _store.WriteAsync(async (session, token) =>
            {
                var job = await session.JobAsync(jobId, token);
                var stored = job?.Artifacts.SingleOrDefault(value => value.ArtifactId == artifact.ArtifactId);
                if (stored is not null) { stored.DeletedAt = Now; job!.Version++; }
                return true;
            }, cancellationToken);
        }
    }

    private FileJobSummary Summary(FileJob job, bool reused = false)
    {
        var value = _mapper.Map<FileJobSummary>(job); value.Reused = reused;
        if (job.Status == "ready" && job.ExpiresAt <= Now) { value.Status = "expired"; value.Artifacts = Array.Empty<FileArtifactSummary>(); }
        return value;
    }
    private bool Owns(FileJob? job, FileJobRequester requester) => ValidRequester(requester) && CanExport(requester) && job is not null
        && job.Environment == _settings.Environment && job.ChurchCode == requester.ChurchCode && job.OwnerHash == requester.OwnerHash;
    private bool GeneratorOwns(FileJob? job, string producer) => job is not null && producer == job.Producer && job.Environment == _settings.Environment;
    private bool ValidAttempt(FileJob? job, string producer, Guid attemptId, string leaseToken) => GeneratorOwns(job, producer)
        && job!.Status is "running" or "cancel_requested" && job.AttemptId == attemptId && job.LeaseUntil > Now && job.AttemptDeadlineAt > Now && SameLease(job, leaseToken);
    private static bool SameLease(FileJob job, string leaseToken) => leaseToken is { Length: 64 } && job.LeaseHash is not null
        && CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(job.LeaseHash), Encoding.ASCII.GetBytes(Hash(leaseToken)));
    private static bool ValidRequester(FileJobRequester requester) => requester.ChurchCode != Guid.Empty && Regex.IsMatch(requester.OwnerHash ?? "", "^[a-f0-9]{64}$");
    private static bool CanExport(FileJobRequester requester) => requester.Roles.Any(role => role is "MEMBER_VIEW" or "ADMIN");
    private static bool Active(string state) => state is "queued" or "running" or "retry_wait" or "cancel_requested";
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    private static DateTime Min(DateTime first, DateTime second) => first <= second ? first : second;
    private static DateTimeOffset Utc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    private void Terminal(IFileJobSession session, FileJob job, string status, string code)
    { job.Status = status; job.FailureCode = code; job.ParametersJson = "{}"; job.UpdatedAt = Now; job.LeaseUntil = null; job.Version++; Audit(session, job, code); }
    private void Retry(IFileJobSession session, FileJob job, string code)
    {
        var delay = _settings.RetryDelaySeconds * (job.AttemptCount <= 1 ? 1 : 4);
        job.Status = "retry_wait"; job.FailureCode = code; job.NextAttemptAt = Now.AddSeconds(delay + RandomNumberGenerator.GetInt32(0, 6));
        job.LeaseUntil = null; job.UpdatedAt = Now; job.Version++;
        Enqueue(session, job, Guid.NewGuid(), job.NextAttemptAt.Value); Audit(session, job, code);
    }
    private void Audit(IFileJobSession session, FileJob job, string code) => session.AddAudit(new FileJobAudit
    { AuditId = Guid.NewGuid(), JobId = job.JobId, AttemptId = job.AttemptId, Status = job.Status, OccurredAt = Now, Code = code });
    private void Enqueue(IFileJobSession session, FileJob job, Guid cause, DateTime available)
    {
        var id = Guid.NewGuid();
        var command = new GenerateFileCommand { MessageId = id, JobId = job.JobId, Environment = job.Environment,
            OccurredAt = _clock.GetUtcNow(), CorrelationId = job.JobId, CausationId = cause, ChurchCode = job.ChurchCode,
            ReportType = job.ReportType, ContractVersion = job.ContractVersion, Producer = job.Producer };
        session.AddMessage(new FileJobOutbox { MessageId = id, JobId = job.JobId, Payload = Encoding.UTF8.GetString(GenerateFileCommandCodec.Encode(command)), CreatedAt = Now, AvailableAt = available });
    }
    private static string MemberParameters(JsonElement parameters)
    {
        var properties = parameters.EnumerateObject().ToArray();
        if (properties.Length > 4 || properties.Select(value => value.Name).Distinct(StringComparer.Ordinal).Count() != properties.Length
            || properties.Any(value => value.Name is not ("fullName" or "stateCode" or "isActive" or "isBaptized")))
            throw new InvalidDataException("Only report filters are accepted.");
        var name = parameters.TryGetProperty("fullName", out var fullName) && fullName.ValueKind != JsonValueKind.Null
            ? fullName.ValueKind == JsonValueKind.String ? fullName.GetString()!.Trim().ToUpperInvariant() : throw new InvalidDataException("Invalid name filter.") : null;
        if (name?.Length > 150) throw new InvalidDataException("Name filter exceeds limit.");
        int? state = null;
        if (parameters.TryGetProperty("stateCode", out var stateCode) && stateCode.ValueKind != JsonValueKind.Null)
        {
            if (stateCode.ValueKind != JsonValueKind.Number || !stateCode.TryGetInt32(out var value) || value < 1) throw new InvalidDataException("Invalid state filter.");
            state = value;
        }
        return JsonSerializer.Serialize(new { fullName = string.IsNullOrEmpty(name) ? null : name, stateCode = state,
            isActive = Boolean("isActive"), isBaptized = Boolean("isBaptized") });
        bool? Boolean(string property)
        {
            if (!parameters.TryGetProperty(property, out var value) || value.ValueKind == JsonValueKind.Null) return null;
            if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False)) throw new InvalidDataException("Invalid boolean filter.");
            return value.GetBoolean();
        }
    }
}
