namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

/// <summary>
/// Conserva la identidad de una intención para respuestas inciertas.
/// </summary>
public sealed class FileJobIntent
{
    /// <summary>
    /// Hash del ambiente, iglesia y solicitante.
    /// </summary>
    /// <example>aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa</example>
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Identificador de intención generado por el cliente.
    /// </summary>
    /// <example>3152bb49-e1aa-4df5-b333-4f96f8b0fc12</example>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Hash del tipo y parámetros de la intención.
    /// </summary>
    /// <example>bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb</example>
    public string Fingerprint { get; set; } = string.Empty;

    /// <summary>
    /// Trabajo que debe devolverse al repetir la intención.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }

    /// <summary>
    /// Fecha UTC de registro de la intención.
    /// </summary>
    /// <example>2026-10-25T12:00:00Z</example>
    public DateTime CreatedAt { get; set; }
}
