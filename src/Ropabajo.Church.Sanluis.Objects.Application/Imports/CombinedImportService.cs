using System.Text.Json;
using Naza.Church.BulkLoads.Contracts;
using Ropabajo.Churc.Sanluis.Framework.Integration;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Ropabajo.Church.Sanluis.Objects.Domain.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Registra una intención autorizada y mensajes pendientes sin publicar antes del commit.
/// </summary>
public sealed class CombinedImportService
{
    private readonly IImportRepository _imports;
    private readonly IBulkLoadRepository _loads;
    private readonly IObjectRepository _objects;
    private readonly IFormatRepository _formats;
    private readonly IIntegrationWork _work;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Recibe repositorios que comparten el contexto y la transacción de Objects.
    /// </summary>
    public CombinedImportService(IImportRepository imports, IBulkLoadRepository loads, IObjectRepository objects, IFormatRepository formats, IIntegrationWork work)
    {
        _imports = imports ?? throw new ArgumentNullException(nameof(imports)); _loads = loads ?? throw new ArgumentNullException(nameof(loads));
        _objects = objects ?? throw new ArgumentNullException(nameof(objects)); _formats = formats ?? throw new ArgumentNullException(nameof(formats));
        _work = work ?? throw new ArgumentNullException(nameof(work));
    }

    /// <summary>
    /// Registra el archivo una vez y conserva el contexto autenticado de su primera solicitud.
    /// </summary>
    public Task<Result<ImportLoadView>> CreateAsync(ImportRequester requester, Guid requestId, Guid objectCode, Guid formatCode, string? description, CancellationToken cancellationToken)
        => _work.ExecuteAsync<Result<ImportLoadView>>(async token =>
    {
        var existing = await _imports.GetAsync(requestId, token);
        if (existing is not null)
        {
            if (existing.ChurchCode != requester.ChurchCode || existing.OwnerHash != requester.OwnerHash)
                return Failure<ImportLoadView>(ImportValidationCodes.IntentConflict, ImportValidationMessages.IntentConflict, ErrorType.Conflict);
            var previous = await _loads.GetOneAsync(x => x.Code == existing.LoadCode && !x.Delete, token);
            if (previous is null || previous.ObjectCode != objectCode || previous.FormatCode != formatCode || previous.Description != description)
                return Failure<ImportLoadView>(ImportValidationCodes.IntentConflict, ImportValidationMessages.IntentConflict, ErrorType.Conflict);
            return new ImportLoadView(previous, existing);
        }
        var file = await _objects.GetOneAsync(x => x.Code == objectCode && !x.Delete, token);
        if (file is null || file.StateCode != Shared.Enums.ObjectState.Uploaded || file.User != requester.Username || file.CreatedBy != requester.UserId)
            return Failure<ImportLoadView>(ImportValidationCodes.ObjectUnavailable, ImportValidationMessages.ObjectUnavailable, ErrorType.NotFound);
        var format = await _formats.GetOneAsync(x => x.Code == formatCode && !x.Delete, token);
        if (format is null || formatCode != Guid.Parse("82d1d1db-0727-421e-a0fe-78750146f433")
            || !file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || format.Path != file.Path
            || format.MaxLength != file.MaxLength || format.AllowedExtensions != file.AllowedExtensions || format.Expiration != file.Expiration)
            return Failure<ImportLoadView>(ImportValidationCodes.FormatInvalid, ImportValidationMessages.FormatInvalid, ErrorType.Validation);
        if (await _loads.GetOneAsync(x => x.ObjectCode == objectCode, token) is not null)
            return Failure<ImportLoadView>(ImportValidationCodes.ObjectUsed, ImportValidationMessages.ObjectUsed, ErrorType.Conflict);
        var scope = new ImportScope { LoadCode = requestId, ChurchCode = requester.ChurchCode, OwnerHash = requester.OwnerHash,
            ActorUser = requester.UserId, ActorIp = requester.Ip };
        var message = new BulkImportRegistered { MessageId = Guid.NewGuid(), CorrelationId = requestId, Scope = scope,
            ObjectCode = file.Code, ObjectName = file.ObjectName, FileName = file.FileName, FormatCode = format.Code, MaxBytes = 10485760 };
        var load = new BulkLoad { Code = requestId, ObjectId = file.Id, ObjectCode = file.Code, ObjectName = file.ObjectName, FileName = file.FileName,
            FormatId = format.Id, FormatCode = format.Code, Description = description, Date = DateTime.UtcNow, StateCode = Shared.Enums.BulkLoadState.Pending,
            User = requester.Username, Records = 0, UploadedRecords = 0, ObservedRecords = 0 };
        await _loads.AddAsync(load, token);
        var metadata = new ImportLoad { LoadCode = requestId, ChurchCode = requester.ChurchCode, OwnerHash = requester.OwnerHash,
            InitialMessageId = message.MessageId, ScopeJson = JsonSerializer.Serialize(scope, Json), Status = "pending" };
        _imports.Add(metadata); _work.Enqueue(message.MessageId, message.CorrelationId, message.Kind, message);
        return new ImportLoadView(load, metadata);
    }, cancellationToken);

    /// <summary>
    /// Exige coincidencia de iglesia y solicitante antes de leer o recuperar filas.
    /// </summary>
    public async Task<Result<ImportLoad>> AuthorizeAsync(ImportRequester requester, Guid loadCode, CancellationToken cancellationToken)
    {
        var metadata = await _imports.GetAsync(loadCode, cancellationToken);
        return metadata is not null && metadata.ChurchCode == requester.ChurchCode && metadata.OwnerHash == requester.OwnerHash
            ? metadata : Failure<ImportLoad>(ImportValidationCodes.NotFound, ImportValidationMessages.NotFound, ErrorType.NotFound);
    }

    /// <summary>
    /// Solicita la recuperación del último paso observado sin modificar directamente sus datos.
    /// </summary>
    public Task<Result<bool>> RetryAsync(ImportRequester requester, Guid loadCode, Guid rowCode, Guid requestId, CancellationToken cancellationToken)
        => _work.ExecuteAsync<Result<bool>>(async token =>
    {
        var authorization = await AuthorizeAsync(requester, loadCode, token);
        if (authorization.IsFailure) return Result.Failure<bool>(authorization.Errors.ToArray());
        var row = await _imports.RowAsync(loadCode, rowCode, token);
        if (row is null) return Failure<bool>(ImportValidationCodes.NotFound, ImportValidationMessages.NotFound, ErrorType.NotFound);
        if (row.Status is "completed" or "person_pending" or "member_pending") return true;
        if (row.Status is not ("member_observed" or "person_observed"))
            return Failure<bool>(ImportValidationCodes.RetryUnavailable, ImportValidationMessages.RetryUnavailable, ErrorType.Conflict);
        var load = await _loads.GetOneAsync(x => x.Code == loadCode && !x.Delete, token) ?? throw new InvalidDataException("Missing registered import.");
        var message = new ImportRowRetryRequested { MessageId = requestId, OccurredAt = DateTime.SpecifyKind(load.Date, DateTimeKind.Utc), CorrelationId = rowCode, RowCode = rowCode,
            Scope = JsonSerializer.Deserialize<ImportScope>(authorization.Value.ScopeJson, Json) ?? throw new InvalidDataException("Invalid stored import scope.") };
        // Preserve payload on an HTTP retry, including its original OccurredAt.
        await _work.ReplayOrEnqueueAsync(message.MessageId, message.CorrelationId, message.Kind, message, token);
        return true;
    }, cancellationToken);

    private static Result<T> Failure<T>(string code, string message, ErrorType type) => Result.Failure<T>(new Error(code, message, type));
}
