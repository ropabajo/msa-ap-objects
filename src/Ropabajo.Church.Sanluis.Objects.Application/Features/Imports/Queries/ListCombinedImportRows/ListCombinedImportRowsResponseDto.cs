namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImportRows;

/// <summary>
/// Resultado propio del caso ListCombinedImportRows.
/// </summary>
public sealed class ListCombinedImportRowsResponseDto
{
    /// <summary>
    /// Elementos autorizados de la página solicitada.
    /// </summary>
    public IReadOnlyList<ListCombinedImportRowsItemResponseDto> Items { get; set; } = [];

    /// <summary>
    /// Cantidad total de elementos autorizados que cumplen el filtro.
    /// </summary>
    /// <example>10</example>
    public int Total { get; set; }
}
