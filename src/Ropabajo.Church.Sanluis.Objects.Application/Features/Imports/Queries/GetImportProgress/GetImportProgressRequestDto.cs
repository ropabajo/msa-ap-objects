using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;

/// <summary>
/// Identificadores visibles en la página; el navegador no decide usuario ni iglesia.
/// </summary>
public sealed class GetImportProgressRequestDto
{
    /// <summary>
    /// Códigos públicos, sin duplicados; máximo cien.
    /// </summary>
    [Required(ErrorMessage = ImportValidationMessages.LoadCodesValid)]
    [MinLength(1, ErrorMessage = ImportValidationMessages.LoadCodesValid)]
    [MaxLength(100, ErrorMessage = ImportValidationMessages.LoadCodesValid)]
    public Guid[] LoadCodes { get; set; } = [];
}
