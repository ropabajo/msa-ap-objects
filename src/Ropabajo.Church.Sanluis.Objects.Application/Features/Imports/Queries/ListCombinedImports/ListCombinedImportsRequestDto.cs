using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImports;

/// <summary>
/// Entrada del caso ListCombinedImports, enlazada con la iglesia autorizada de la ruta.
/// </summary>
public sealed class ListCombinedImportsRequestDto
{
    /// <summary>
    /// Identificador de la iglesia autorizado por la ruta.
    /// </summary>
    /// <example>64f46864-aad9-47ca-b499-af5e2384e70a</example>
    [Required(ErrorMessage = ImportValidationMessages.ChurchCodeRequired)]
    public Guid ChurchCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Formato opcional para filtrar las cargas.
    /// </summary>
    /// <example>82d1d1db-0727-421e-a0fe-78750146f433</example>
    public Guid? FilterFormatCode { get; set; } = null;

    /// <summary>
    /// Número de página, empezando en uno.
    /// </summary>
    /// <example>1</example>
    [Range(1, 100000, ErrorMessage = ImportValidationMessages.PageNumberRange)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Cantidad máxima de elementos de la página.
    /// </summary>
    /// <example>10</example>
    [Range(1, 100, ErrorMessage = ImportValidationMessages.PageSizeRange)]
    public int PageSize { get; set; } = 10;
}
