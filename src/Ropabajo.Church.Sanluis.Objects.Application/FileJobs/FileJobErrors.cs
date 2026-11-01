using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs;

/// <summary>
/// Centraliza los fallos seguros del protocolo de trabajos.
/// </summary>
public static class FileJobErrors
{
    /// <summary>
    /// El mecanismo no ha sido activado en el ambiente.
    /// </summary>
    public static Error Disabled => new("FILE_JOBS_DISABLED", "La exportación no está disponible por el momento.", ErrorType.Unavailable);

    /// <summary>
    /// El recurso no existe en el ámbito autorizado.
    /// </summary>
    public static Error NotFound => new("FILE_JOB_NOT_FOUND", "No se encontró el trabajo solicitado.", ErrorType.NotFound);

    /// <summary>
    /// La solicitud contiene un contrato o parámetros no válidos.
    /// </summary>
    public static Error Invalid => new("FILE_JOB_INVALID", "Los datos de la solicitud no son válidos.", ErrorType.Validation);

    /// <summary>
    /// El usuario no tiene permiso para exportar este reporte.
    /// </summary>
    public static Error Forbidden => new("FILE_JOB_FORBIDDEN", "No tienes permiso para exportar este reporte.", ErrorType.Forbidden);

    /// <summary>
    /// El usuario superó su cuota temporal de trabajos activos.
    /// </summary>
    public static Error Quota => new("FILE_JOB_QUOTA", "Hay varias exportaciones en preparación. Espera a que terminen.", ErrorType.RateLimited);

    /// <summary>
    /// La intención se utilizó con otros parámetros.
    /// </summary>
    public static Error IntentConflict => new("FILE_JOB_INTENT_CONFLICT", "La solicitud ya fue utilizada con otros filtros.", ErrorType.Conflict);

    /// <summary>
    /// Existe un intento activo que todavía conserva el lease.
    /// </summary>
    public static Error Busy => new("FILE_JOB_LEASE_ACTIVE", "El trabajo está siendo procesado por otro intento.", ErrorType.Conflict);

    /// <summary>
    /// La entrega corresponde a un trabajo que ya terminó.
    /// </summary>
    public static Error Terminal => new("FILE_JOB_TERMINAL", "El trabajo ya terminó y no requiere otra generación.", ErrorType.Conflict);

    /// <summary>
    /// El intento dejó de tener autoridad para publicar.
    /// </summary>
    public static Error Stale => new("FILE_JOB_ATTEMPT_STALE", "El intento dejó de ser válido.", ErrorType.Conflict);

    /// <summary>
    /// Los archivos todavía no están publicados o su disponibilidad terminó.
    /// </summary>
    public static Error Unavailable => new("FILE_JOB_NOT_READY", "El archivo todavía no está disponible o su vigencia terminó.", ErrorType.Conflict);

    /// <summary>
    /// El conjunto de archivos no coincide con lo esperado o no superó la verificación.
    /// </summary>
    public static Error InvalidFiles => new("FILE_JOB_INVALID_FILES", "Los archivos no superaron la comprobación de integridad.", ErrorType.BusinessRule);

    /// <summary>
    /// Una dependencia de almacenamiento está temporalmente indisponible.
    /// </summary>
    public static Error Storage => new("FILE_STORAGE_UNAVAILABLE", "El almacenamiento no está disponible por el momento.", ErrorType.Unavailable);
}
