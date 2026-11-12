using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;
/// <summary>
/// Valida el identificador de la carga que se confirma.
/// </summary>
public sealed class UploadObjectCommandValidator : AbstractValidator<UploadObjectCommand>
{
    /// <summary>
    /// Registra las reglas con mensajes funcionales y códigos estables.
    /// </summary>
    public UploadObjectCommandValidator() => RuleFor(x => x.ObjectCode)
        .NotEmpty().WithMessage(ObjectUploadValidationMessages.ObjectCodeRequired).WithErrorCode(ObjectUploadValidationCodes.ObjectCodeRequired)
        .Must(code => Guid.TryParse(code, out _)).WithMessage(ObjectUploadValidationMessages.ObjectCodeInvalid).WithErrorCode(ObjectUploadValidationCodes.ObjectCodeInvalid);
}
