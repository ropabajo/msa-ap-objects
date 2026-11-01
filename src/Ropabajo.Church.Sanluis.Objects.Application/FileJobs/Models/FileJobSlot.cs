using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

/// <summary>
/// Serializa la reutilización de una búsqueda mediante una clave única.
/// </summary>
[Table("tarea_archivo_reutilizacion")]
public sealed class FileJobSlot
{
    /// <summary>
    /// Identificador interno autoincremental del registro.
    /// </summary>
    /// <example>1</example>
    [Key]
    [Column("ID_TAREA_ARCHIVO_REUTILIZACION")]
    public long Id { get; set; }

    /// <summary>
    /// Hash completo del ámbito y búsqueda.
    /// </summary>
    /// <example>aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa</example>
    [Column("HUELLA_SOLICITUD")]
    public string Fingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Trabajo activo más reciente de la búsqueda.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    [Column("CODIGO_TAREA_ARCHIVO")]
    public Guid JobId { get; set; }

    /// <summary>
    /// Versión del registro de reutilización.
    /// </summary>
    /// <example>1</example>
    [Column("VERSION")]
    public long Version { get; set; }
}
