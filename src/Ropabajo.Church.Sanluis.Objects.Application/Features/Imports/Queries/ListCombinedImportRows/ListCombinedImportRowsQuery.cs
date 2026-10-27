using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImportRows;

/// <summary>
/// Entrada del caso ListCombinedImportRows, enlazada con la iglesia autorizada de la ruta.
/// </summary>
public sealed class ListCombinedImportRowsQuery : IRequest<Result<ListCombinedImportRowsResponseDto>>
{
    /// <summary>
    /// Identificador de la iglesia autorizado por la ruta.
    /// </summary>
    /// <example>64f46864-aad9-47ca-b499-af5e2384e70a</example>
    public Guid ChurchCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Código público de la carga propia.
    /// </summary>
    /// <example>8431e4c1-4a16-46ab-8932-e02851c073dd</example>
    public Guid LoadCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Estado funcional opcional por el que filtrar filas.
    /// </summary>
    /// <example>OBSERVADO</example>
    public string? StateCode { get; set; } = null;

    /// <summary>
    /// Número de página, empezando en uno.
    /// </summary>
    /// <example>1</example>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Cantidad máxima de elementos de la página.
    /// </summary>
    /// <example>10</example>
    public int PageSize { get; set; } = 10;
}
