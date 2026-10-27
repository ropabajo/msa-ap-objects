using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImports;

/// <summary>
/// Valida identificadores y límites de la solicitud de importación.
/// </summary>
public sealed class ListCombinedImportsQueryValidator : AbstractValidator<ListCombinedImportsQuery>
{
    /// <summary>
    /// Registra las reglas funcionales de entrada.
    /// </summary>
    public ListCombinedImportsQueryValidator()
    {
        RuleFor(x => x.ChurchCode).NotEmpty().WithMessage(ImportValidationMessages.ChurchCodeRequired).WithErrorCode(ImportValidationCodes.ChurchCodeRequired);
        RuleFor(x => x.PageNumber).InclusiveBetween(1, 100000).WithMessage(ImportValidationMessages.PageNumberRange).WithErrorCode(ImportValidationCodes.PageNumberRange);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage(ImportValidationMessages.PageSizeRange).WithErrorCode(ImportValidationCodes.PageSizeRange);
    }
}
