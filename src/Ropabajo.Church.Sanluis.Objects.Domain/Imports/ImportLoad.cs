namespace Ropabajo.Church.Sanluis.Objects.Domain.Imports;

/// <summary>
/// Seguimiento durable de ImportLoad para completar Persona y Miembro.
/// </summary>
public sealed class ImportLoad
{
    /// <summary>
    /// Código público de carga.
    /// </summary>
    public Guid LoadCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Iglesia autorizada.
    /// </summary>
    public Guid ChurchCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Huella del solicitante.
    /// </summary>
    public string OwnerHash { get; set; } = string.Empty;

    /// <summary>
    /// Contexto autorizado serializado.
    /// </summary>
    public string ScopeJson { get; set; } = string.Empty;

    /// <summary>
    /// Identidad estable del registro.
    /// </summary>
    public Guid InitialMessageId { get; set; } = Guid.Empty;

    /// <summary>
    /// Estado del lote.
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Código seguro del error del archivo.
    /// </summary>
    public string? ErrorCode { get; set; } = null;

    /// <summary>
    /// Última modificación UTC.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Versión para concurrencia.
    /// </summary>
    public long Version { get; set; } = 0;

}
