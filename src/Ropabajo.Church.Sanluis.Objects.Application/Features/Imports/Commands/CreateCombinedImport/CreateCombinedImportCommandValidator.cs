using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateCombinedImport;

/// <summary>
/// Valida identificadores y límites de la solicitud de importación.
/// </summary>
public sealed class CreateCombinedImportCommandValidator : AbstractValidator<CreateCombinedImportCommand>
{
    /// <summary>
    /// Registra las reglas funcionales de entrada.
    /// </summary>
    public CreateCombinedImportCommandValidator()
    {
        RuleFor(x => x.ChurchCode).NotEmpty().WithMessage(ImportValidationMessages.ChurchCodeRequired).WithErrorCode(ImportValidationCodes.ChurchCodeRequired);
        RuleFor(x => x.RequestId).NotEmpty().WithMessage(ImportValidationMessages.RequestIdRequired).WithErrorCode(ImportValidationCodes.RequestIdRequired);
        RuleFor(x => x.ObjectCode).NotEmpty().WithMessage(ImportValidationMessages.ObjectCodeRequired).WithErrorCode(ImportValidationCodes.ObjectCodeRequired);
        RuleFor(x => x.FormatCode).NotEmpty().WithMessage(ImportValidationMessages.FormatCodeRequired).WithErrorCode(ImportValidationCodes.FormatCodeRequired);
        RuleFor(x => x.Description).MaximumLength(400).WithMessage(ImportValidationMessages.DescriptionMaxLength).WithErrorCode(ImportValidationCodes.DescriptionMaxLength);
    }
}
