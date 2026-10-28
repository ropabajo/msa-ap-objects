namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImports;

/// <summary>
/// Elemento funcional de la página autorizada.
/// </summary>
public sealed class ListCombinedImportsItemResponseDto
{
    /// <summary>Versión confirmada; permite descartar actualizaciones recibidas fuera de orden.</summary>
    public long Version { get; set; }
    /// <summary>Fecha UTC del último cambio confirmado de la carga.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Código público de carga.
    /// </summary>
    /// <example>8431e4c1-4a16-46ab-8932-e02851c073dd</example>
    public Guid BulkLoadCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Descripción de la carga.
    /// </summary>
    /// <example>Carga de personas y miembros</example>
    public string? Description { get; set; } = null;

    /// <summary>
    /// Estado general compatible con la bandeja.
    /// </summary>
    /// <example>PROCESANDO</example>
    public string StateCode { get; set; } = string.Empty;

    /// <summary>
    /// Estado del flujo Persona y Miembro.
    /// </summary>
    /// <example>processing</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Error seguro de archivo cuando existe.
    /// </summary>
    /// <example>IMPORT_WORKBOOK_INVALID</example>
    public string? ErrorCode { get; set; } = null;

    /// <summary>
    /// Fecha UTC de registro.
    /// </summary>
    /// <example>2026-10-26T20:00:00Z</example>
    public DateTime Date { get; set; } = default;

    /// <summary>
    /// Total de filas leídas.
    /// </summary>
    /// <example>10</example>
    public int? Records { get; set; } = null;

    /// <summary>
    /// Filas con Miembro confirmado.
    /// </summary>
    /// <example>8</example>
    public int? UploadedRecords { get; set; } = null;

    /// <summary>
    /// Filas con observaciones.
    /// </summary>
    /// <example>2</example>
    public int? ObservedRecords { get; set; } = null;

    /// <summary>
    /// Usuario que registró la carga.
    /// </summary>
    /// <example>operador-dev</example>
    public string User { get; set; } = string.Empty;

}
