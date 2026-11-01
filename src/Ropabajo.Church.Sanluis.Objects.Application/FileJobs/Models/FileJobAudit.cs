using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

/// <summary>
/// Registra una transición sin nombres, documentos o contenido del reporte.
/// </summary>
[Table("tarea_archivo_auditoria")]
public sealed class FileJobAudit
{
    /// <summary>
    /// Identificador interno autoincremental del registro.
    /// </summary>
    /// <example>1</example>
    [Key]
    [Column("ID_TAREA_ARCHIVO_AUDITORIA")]
    public long Id { get; set; }

    /// <summary>
    /// Identificador del evento de auditoría.
    /// </summary>
    /// <example>bb7218a5-7a31-4342-b671-1687f13cdc0e</example>
    [Column("CODIGO_AUDITORIA")]
    public Guid AuditId { get; set; }

    /// <summary>
    /// Trabajo al que corresponde la transición.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    [Column("CODIGO_TAREA_ARCHIVO")]
    public Guid JobId { get; set; }

    /// <summary>
    /// Intento relacionado con la transición.
    /// </summary>
    /// <example>fa8e9db0-c458-48ba-81e2-d54d94655983</example>
    [Column("CODIGO_INTENTO")]
    public Guid? AttemptId { get; set; }

    /// <summary>
    /// Estado técnico registrado.
    /// </summary>
    /// <example>queued</example>
    [Column("CODIGO_ESTADO")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Fecha UTC de la transición.
    /// </summary>
    /// <example>2026-10-25T12:00:00Z</example>
    [Column("FECHA_EVENTO")]
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Código seguro de la transición o fallo.
    /// </summary>
    /// <example>FILE_JOB_CREATED</example>
    [Column("CODIGO_EVENTO")]
    public string? Code { get; set; }
}
