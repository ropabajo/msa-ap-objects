using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

/// <summary>
/// Conserva el estado durable y el ámbito de un trabajo de archivos.
/// </summary>
[Table("tarea_archivo")]
public sealed class FileJob
{
    /// <summary>
    /// Identificador público del trabajo.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    [Column("CODIGO_TAREA_ARCHIVO")]
    public Guid JobId { get; set; }

    /// <summary>
    /// Secuencia interna para paginación estable.
    /// </summary>
    /// <example>125</example>
    [Key]
    [Column("ID_TAREA_ARCHIVO")]
    public long Sequence { get; set; }

    /// <summary>
    /// Ambiente lógico que posee el trabajo.
    /// </summary>
    /// <example>development</example>
    [Column("AMBIENTE")]
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Código de la iglesia autorizada.
    /// </summary>
    /// <example>8cbddf7f-a8ab-46a0-a832-373f454357cd</example>
    [Column("CODIGO_IGLESIA")]
    public Guid ChurchCode { get; set; }

    /// <summary>
    /// Hash del emisor y sujeto autenticado, sin guardar el JWT.
    /// </summary>
    /// <example>aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa</example>
    [Column("HUELLA_SOLICITANTE")]
    public string OwnerHash { get; set; } = string.Empty;

    /// <summary>
    /// Reporte registrado que determina generador y política.
    /// </summary>
    /// <example>members.excel</example>
    [Column("TIPO_REPORTE")]
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Versión de parámetros del reporte.
    /// </summary>
    /// <example>1</example>
    [Column("VERSION_CONTRATO")]
    public int ContractVersion { get; set; } = 1;

    /// <summary>
    /// Servicio autorizado para generar el contenido.
    /// </summary>
    /// <example>church</example>
    [Column("SERVICIO_GENERADOR")]
    public string Producer { get; set; } = string.Empty;

    /// <summary>
    /// Hash de ámbito, tipo, plantilla y parámetros normalizados.
    /// </summary>
    /// <example>bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb</example>
    [Column("HUELLA_SOLICITUD")]
    public string Fingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Parámetros pequeños del reporte, nunca una lista de registros.
    /// </summary>
    /// <example>{"fullName":"PÉREZ"}</example>
    [Column("PARAMETROS")]
    public string ParametersJson { get; set; } = string.Empty;

    /// <summary>
    /// Estado técnico persistido del trabajo.
    /// </summary>
    /// <example>queued</example>
    [Column("CODIGO_ESTADO")]
    public string Status { get; set; } = "queued";

    /// <summary>
    /// Fecha UTC de aceptación durable.
    /// </summary>
    /// <example>2026-10-25T12:00:00Z</example>
    [Column("FECHA_CREACION")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha UTC de la última transición.
    /// </summary>
    /// <example>2026-10-25T12:00:01Z</example>
    [Column("FECHA_MODIFICACION")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Plazo máximo UTC para completar el trabajo.
    /// </summary>
    /// <example>2026-10-25T13:00:00Z</example>
    [Column("FECHA_LIMITE")]
    public DateTime DeadlineAt { get; set; }

    /// <summary>
    /// Fecha UTC de emisión del archivo publicado.
    /// </summary>
    /// <example>2026-10-25T12:00:10Z</example>
    [Column("FECHA_GENERACION")]
    public DateTime? GeneratedAt { get; set; }

    /// <summary>
    /// Fecha UTC de los datos representados.
    /// </summary>
    /// <example>2026-10-25T12:00:06Z</example>
    [Column("FECHA_DATOS")]
    public DateTime? DataAsOf { get; set; }

    /// <summary>
    /// Fin UTC de disponibilidad de los archivos temporales.
    /// </summary>
    /// <example>2026-10-26T12:00:10Z</example>
    [Column("FECHA_EXPIRACION")]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Cantidad de intentos iniciados.
    /// </summary>
    /// <example>1</example>
    [Column("NUMERO_INTENTOS")]
    public int AttemptCount { get; set; }

    /// <summary>
    /// Identificador del intento vigente.
    /// </summary>
    /// <example>fa8e9db0-c458-48ba-81e2-d54d94655983</example>
    [Column("CODIGO_INTENTO")]
    public Guid? AttemptId { get; set; }

    /// <summary>
    /// Hash de la credencial temporal del intento.
    /// </summary>
    /// <example>cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc</example>
    [Column("HUELLA_PERMISO_INTENTO")]
    public string? LeaseHash { get; set; }

    /// <summary>
    /// Vencimiento UTC del permiso del intento.
    /// </summary>
    /// <example>2026-10-25T12:02:00Z</example>
    [Column("FECHA_EXPIRACION_PERMISO")]
    public DateTime? LeaseUntil { get; set; }

    /// <summary>
    /// Límite UTC no renovable de ejecución del intento.
    /// </summary>
    /// <example>2026-10-25T12:15:00Z</example>
    [Column("FECHA_LIMITE_INTENTO")]
    public DateTime? AttemptDeadlineAt { get; set; }

    /// <summary>
    /// Fecha UTC de un reintento programado.
    /// </summary>
    /// <example>2026-10-25T12:01:00Z</example>
    [Column("FECHA_SIGUIENTE_INTENTO")]
    public DateTime? NextAttemptAt { get; set; }

    /// <summary>
    /// Código seguro del fallo, sin mensajes privados del proveedor.
    /// </summary>
    /// <example>FILE_GENERATION_FAILED</example>
    [Column("CODIGO_ERROR")]
    public string? FailureCode { get; set; }

    /// <summary>
    /// Disponibilidad del reporte desde su generación; nulo conserva el histórico.
    /// </summary>
    /// <example>24</example>
    [Column("HORAS_CONSERVACION")]
    public int? RetentionHours { get; set; } = 24;

    /// <summary>
    /// Antigüedad máxima de datos para reutilizar un trabajo listo.
    /// </summary>
    /// <example>300</example>
    [Column("SEGUNDOS_REUTILIZACION")]
    public int ReuseSeconds { get; set; } = 300;

    /// <summary>
    /// Versión de concurrencia para impedir transiciones obsoletas.
    /// </summary>
    /// <example>1</example>
    [Column("VERSION")]
    public long Version { get; set; }

    /// <summary>
    /// Archivos reservados o publicados por los intentos del trabajo.
    /// </summary>
    public List<FileJobArtifact> Artifacts { get; set; } = new();
}
