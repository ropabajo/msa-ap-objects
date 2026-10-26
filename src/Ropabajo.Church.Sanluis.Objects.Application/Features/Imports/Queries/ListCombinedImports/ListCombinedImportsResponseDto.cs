namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImports;

/// <summary>
/// Resultado propio del caso ListCombinedImports.
/// </summary>
public sealed class ListCombinedImportsResponseDto
{
    /// <summary>
    /// Elementos autorizados de la página solicitada.
    /// </summary>
    public IReadOnlyList<ListCombinedImportsItemResponseDto> Items { get; set; } = [];

    /// <summary>
    /// Cantidad total de elementos autorizados que cumplen el filtro.
    /// </summary>
    /// <example>10</example>
    public int Total { get; set; }
}
