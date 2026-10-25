namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

/// <summary>
/// Serializa la reutilización de una búsqueda mediante una clave única.
/// </summary>
public sealed class FileJobSlot
{
    /// <summary>
    /// Hash completo del ámbito y búsqueda.
    /// </summary>
    /// <example>aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa</example>
    public string Fingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Trabajo activo más reciente de la búsqueda.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }

    /// <summary>
    /// Versión del registro de reutilización.
    /// </summary>
    /// <example>1</example>
    public long Version { get; set; }
}
