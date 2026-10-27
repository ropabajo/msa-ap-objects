namespace Ropabajo.Church.Sanluis.Objects.Domain.Imports;

/// <summary>
/// Seguimiento durable de ImportRow para completar Persona y Miembro.
/// </summary>
public sealed class ImportRow
{
    /// <summary>Identidad numérica interna; el código público existente se conserva.</summary>
    public long Id { get; set; }

    /// <summary>
    /// Código público de fila.
    /// </summary>
    public Guid RowCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Carga propietaria.
    /// </summary>
    public Guid LoadCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Número real de fila.
    /// </summary>
    public int RowNumber { get; set; } = 0;

    /// <summary>
    /// Solicitud estable de Persona.
    /// </summary>
    public Guid PersonRequestId { get; set; } = Guid.Empty;

    /// <summary>
    /// Solicitud vigente de Miembro.
    /// </summary>
    public Guid? MemberRequestId { get; set; } = null;

    /// <summary>
    /// Persona confirmada.
    /// </summary>
    public Guid? PersonCode { get; set; } = null;

    /// <summary>
    /// Miembro confirmado.
    /// </summary>
    public Guid? MemberCode { get; set; } = null;

    /// <summary>
    /// Solicitud original para recuperación.
    /// </summary>
    public string PersonRequestJson { get; set; } = string.Empty;

    /// <summary>
    /// Solicitud del paso Miembro.
    /// </summary>
    public string? MemberRequestJson { get; set; } = null;

    /// <summary>
    /// Paso funcional actual.
    /// </summary>
    public string Status { get; set; } = "person_pending";

    /// <summary>
    /// Código seguro de observación.
    /// </summary>
    public string? ErrorCode { get; set; } = null;

    /// <summary>
    /// Detalle seguro de observación.
    /// </summary>
    public string? ErrorMessage { get; set; } = null;

    /// <summary>
    /// Última modificación UTC.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Versión para concurrencia.
    /// </summary>
    public long Version { get; set; } = 0;

}
