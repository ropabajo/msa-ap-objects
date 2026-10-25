namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

/// <summary>
/// Conserva el estado durable y el ámbito de un trabajo de archivos.
/// </summary>
public sealed class FileJob
{
    /// <summary>
    /// Identificador público del trabajo.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }

    /// <summary>
    /// Secuencia interna para paginación estable.
    /// </summary>
    /// <example>125</example>
    public long Sequence { get; set; }

    /// <summary>
    /// Ambiente lógico que posee el trabajo.
    /// </summary>
    /// <example>development</example>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Código de la iglesia autorizada.
    /// </summary>
    /// <example>8cbddf7f-a8ab-46a0-a832-373f454357cd</example>
    public Guid ChurchCode { get; set; }

    /// <summary>
    /// Hash del emisor y sujeto autenticado, sin guardar el JWT.
    /// </summary>
    /// <example>aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa</example>
    public string OwnerHash { get; set; } = string.Empty;

    /// <summary>
    /// Reporte registrado que determina generador y política.
    /// </summary>
    /// <example>members.excel</example>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Versión de parámetros del reporte.
    /// </summary>
    /// <example>1</example>
    public int ContractVersion { get; set; } = 1;

    /// <summary>
    /// Servicio autorizado para generar el contenido.
    /// </summary>
    /// <example>church</example>
    public string Producer { get; set; } = string.Empty;

    /// <summary>
    /// Hash de ámbito, tipo, plantilla y parámetros normalizados.
    /// </summary>
    /// <example>bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb</example>
    public string Fingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Parámetros pequeños del reporte, nunca una lista de registros.
    /// </summary>
    /// <example>{"fullName":"PÉREZ"}</example>
    public string ParametersJson { get; set; } = string.Empty;

    /// <summary>
    /// Estado técnico persistido del trabajo.
    /// </summary>
    /// <example>queued</example>
    public string Status { get; set; } = "queued";

    /// <summary>
    /// Fecha UTC de aceptación durable.
    /// </summary>
    /// <example>2026-10-25T12:00:00Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha UTC de la última transición.
    /// </summary>
    /// <example>2026-10-25T12:00:01Z</example>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Plazo máximo UTC para completar el trabajo.
    /// </summary>
    /// <example>2026-10-25T13:00:00Z</example>
    public DateTime DeadlineAt { get; set; }

    /// <summary>
    /// Fecha UTC de emisión del archivo publicado.
    /// </summary>
    /// <example>2026-10-25T12:00:10Z</example>
    public DateTime? GeneratedAt { get; set; }

    /// <summary>
    /// Fecha UTC de los datos representados.
    /// </summary>
    /// <example>2026-10-25T12:00:06Z</example>
    public DateTime? DataAsOf { get; set; }

    /// <summary>
    /// Fin UTC de disponibilidad de los archivos temporales.
    /// </summary>
    /// <example>2026-10-26T12:00:10Z</example>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Cantidad de intentos iniciados.
    /// </summary>
    /// <example>1</example>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Identificador del intento vigente.
    /// </summary>
    /// <example>fa8e9db0-c458-48ba-81e2-d54d94655983</example>
    public Guid? AttemptId { get; set; }

    /// <summary>
    /// Hash de la credencial temporal del intento.
    /// </summary>
    /// <example>cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc</example>
    public string? LeaseHash { get; set; }

    /// <summary>
    /// Vencimiento UTC del permiso del intento.
    /// </summary>
    /// <example>2026-10-25T12:02:00Z</example>
    public DateTime? LeaseUntil { get; set; }

    /// <summary>
    /// Límite UTC no renovable de ejecución del intento.
    /// </summary>
    /// <example>2026-10-25T12:15:00Z</example>
    public DateTime? AttemptDeadlineAt { get; set; }

    /// <summary>
    /// Fecha UTC de un reintento programado.
    /// </summary>
    /// <example>2026-10-25T12:01:00Z</example>
    public DateTime? NextAttemptAt { get; set; }

    /// <summary>
    /// Código seguro del fallo, sin mensajes privados del proveedor.
    /// </summary>
    /// <example>FILE_GENERATION_FAILED</example>
    public string? FailureCode { get; set; }

    /// <summary>
    /// Disponibilidad del reporte desde su generación; nulo conserva el histórico.
    /// </summary>
    /// <example>24</example>
    public int? RetentionHours { get; set; } = 24;

    /// <summary>
    /// Antigüedad máxima de datos para reutilizar un trabajo listo.
    /// </summary>
    /// <example>300</example>
    public int ReuseSeconds { get; set; } = 300;

    /// <summary>
    /// Versión de concurrencia para impedir transiciones obsoletas.
    /// </summary>
    /// <example>1</example>
    public long Version { get; set; }

    /// <summary>
    /// Archivos reservados o publicados por los intentos del trabajo.
    /// </summary>
    public List<FileJobArtifact> Artifacts { get; set; } = new();
}
