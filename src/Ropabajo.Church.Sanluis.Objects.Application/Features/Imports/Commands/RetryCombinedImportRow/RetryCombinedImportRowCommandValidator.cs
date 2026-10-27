using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.RetryCombinedImportRow;

/// <summary>
/// Valida identificadores y límites de la solicitud de importación.
/// </summary>
public sealed class RetryCombinedImportRowCommandValidator : AbstractValidator<RetryCombinedImportRowCommand>
{
    /// <summary>
    /// Registra las reglas funcionales de entrada.
    /// </summary>
    public RetryCombinedImportRowCommandValidator()
    {
        RuleFor(x => x.ChurchCode).NotEmpty().WithMessage(ImportValidationMessages.ChurchCodeRequired).WithErrorCode(ImportValidationCodes.ChurchCodeRequired);
        RuleFor(x => x.LoadCode).NotEmpty().WithMessage(ImportValidationMessages.LoadCodeRequired).WithErrorCode(ImportValidationCodes.LoadCodeRequired);
        RuleFor(x => x.RowCode).NotEmpty().WithMessage(ImportValidationMessages.RowCodeRequired).WithErrorCode(ImportValidationCodes.RowCodeRequired);
        RuleFor(x => x.RequestId).NotEmpty().WithMessage(ImportValidationMessages.RequestIdRequired).WithErrorCode(ImportValidationCodes.RequestIdRequired);
    }
}
