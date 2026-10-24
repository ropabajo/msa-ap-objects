namespace Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;

/// <summary>Ámbito obtenido del token validado y la iglesia de la ruta, nunca del cuerpo.</summary>
/// <param name="ChurchCode">Iglesia activa autorizada.</param>
/// <param name="Owner">SHA-256 del emisor y subject del token.</param>
public sealed record ExportScope(Guid ChurchCode, string Owner);

/// <summary>Manifiesto privado durable, sin credenciales ni tokens del usuario.</summary>
/// <param name="Id">Identidad única del archivo.</param>
/// <param name="Scope">Propietario e iglesia.</param>
/// <param name="RequestId">Primera intención que reservó el trabajo.</param>
/// <param name="Fingerprint">Búsqueda y versión del reporte.</param>
/// <param name="Kind">Familia del XLSX, por ejemplo members-v1.</param>
/// <param name="Status">pending, ready o failed.</param>
/// <param name="CreatedAt">Solicitud UTC.</param>
/// <param name="SourceDeadline">Plazo de recepción del JSON inicial.</param>
/// <param name="GeneratedAt">Emisión efectiva UTC.</param>
/// <param name="ExpiresAt">Cierre lógico de disponibilidad.</param>
/// <param name="DataAsOf">Instante del snapshot de negocio.</param>
/// <param name="FileName">Nombre seguro de descarga.</param>
/// <param name="Attempts">Intentos de generación; un fallo no bloquea otros trabajos.</param>
public sealed record ExportJob(Guid Id, ExportScope Scope, Guid RequestId, string Fingerprint, string Kind,
    string Status, DateTimeOffset CreatedAt, DateTimeOffset SourceDeadline, DateTimeOffset? GeneratedAt,
    DateTimeOffset ExpiresAt, DateTimeOffset? DataAsOf, string FileName, int Attempts = 0);

/// <summary>Índice temporal de idempotencia o de búsqueda.</summary>
/// <param name="JobKey">Ruta interna del manifiesto dentro del prefijo temporal.</param>
/// <param name="Fingerprint">Evita reutilizar RequestId con filtros distintos.</param>
/// <param name="ExpiresAt">Fin de retención del índice.</param>
public sealed record ExportIndex(string JobKey, string Fingerprint, DateTimeOffset ExpiresAt);

/// <summary>Proveedor central de objetos privados. Las claves están restringidas al prefijo de exportaciones.</summary>
public interface IExportObjectStore
{
    /// <summary>Lee JSON o datos internos; null significa objeto inexistente.</summary>
    Task<byte[]?> ReadAsync(string key, CancellationToken token);
    /// <summary>Publica un objeto completo mediante un único PUT.</summary>
    Task WriteAsync(string key, byte[] content, string contentType, CancellationToken token);
    /// <summary>Comprueba existencia sin descargar el XLSX.</summary>
    Task<bool> ExistsAsync(string key, CancellationToken token);
    /// <summary>Enumera claves bajo el único prefijo permitido.</summary>
    Task<IReadOnlyList<string>> ListAsync(string prefix, CancellationToken token);
    /// <summary>Borra únicamente una clave temporal; borrar una inexistente es idempotente.</summary>
    Task DeleteAsync(string key, CancellationToken token);
    /// <summary>Firma una descarga con MIME y nombre de archivo controlados.</summary>
    Task<string> SignAsync(string key, string fileName, int seconds, DateTimeOffset issuedAt, CancellationToken token);
}

/// <summary>Exclusión entre réplicas para reserva, generación y limpieza, sin crear tablas.</summary>
public interface IExportLeaseFactory
{
    /// <summary>Intenta obtener un bloqueo; null permite reintentar sin duplicar trabajo.</summary>
    Task<IAsyncDisposable?> TryAcquireAsync(string key, CancellationToken token);
}

/// <summary>Identifica al usuario autenticado desde el adaptador HTTP.</summary>
public interface IExportActor
{
    /// <summary>Resuelve identidad y ámbito; una llamada anónima nunca produce un propietario válido.</summary>
    ExportScope GetScope(Guid churchCode);
}

/// <summary>Representación XLSX genérica; las reglas de miembros pertenecen a Church.</summary>
public interface ITemporaryWorkbookWriter
{
    /// <summary>Genera texto seguro, números y fechas nativas con el instante de emisión.</summary>
    byte[] Write(ExportWorkbookSource source, DateTimeOffset generatedAt, CancellationToken token);
}

/// <summary>Error de flujo conocido, traducido a HTTP por el adaptador de la API.</summary>
/// <param name="Code">invalid, missing, expired, busy, pending, disabled o unavailable.</param>
/// <param name="Message">Mensaje seguro y accionable para el usuario.</param>
public sealed class ExportFlowException(string Code, string Message) : Exception(Message)
{
    /// <summary>Categoría estable, por ejemplo expired.</summary>
    public string Code { get; } = Code;
}
