using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImportRows;

/// <summary>
/// Valida identificadores y límites de la solicitud de importación.
/// </summary>
public sealed class ListCombinedImportRowsQueryValidator : AbstractValidator<ListCombinedImportRowsQuery>
{
    /// <summary>
    /// Registra las reglas funcionales de entrada.
    /// </summary>
    public ListCombinedImportRowsQueryValidator()
    {
        RuleFor(x => x.ChurchCode).NotEmpty().WithMessage(ImportValidationMessages.ChurchCodeRequired).WithErrorCode(ImportValidationCodes.ChurchCodeRequired);
        RuleFor(x => x.LoadCode).NotEmpty().WithMessage(ImportValidationMessages.LoadCodeRequired).WithErrorCode(ImportValidationCodes.LoadCodeRequired);
        RuleFor(x => x.StateCode).Must(x => x is null or "" or "OBSERVADO" or "PROCESADO" or "COMPLETADO").WithMessage(ImportValidationMessages.StateInvalid).WithErrorCode(ImportValidationCodes.StateInvalid);
        RuleFor(x => x.PageNumber).InclusiveBetween(1, 100000).WithMessage(ImportValidationMessages.PageNumberRange).WithErrorCode(ImportValidationCodes.PageNumberRange);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage(ImportValidationMessages.PageSizeRange).WithErrorCode(ImportValidationCodes.PageSizeRange);
    }
}
