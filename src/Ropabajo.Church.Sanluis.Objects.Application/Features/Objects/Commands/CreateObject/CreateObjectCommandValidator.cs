using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;
namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;
/// <summary>
/// Valida los metadatos de la reserva; el servidor aplica además su política de fotos.
/// </summary>
public sealed class CreateObjectCommandValidator : AbstractValidator<CreateObjectCommand>
{
    /// <summary>
    /// Registra las reglas y códigos compartidos con el contrato HTTP.
    /// </summary>
    public CreateObjectCommandValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage(ObjectUploadValidationMessages.PathRequired).WithErrorCode(ObjectUploadValidationCodes.PathRequired)
            .MaximumLength(400).WithMessage(ObjectUploadValidationMessages.PathMaxLength).WithErrorCode(ObjectUploadValidationCodes.PathMaxLength);
        RuleFor(x => x.AllowedExtensions).NotEmpty().WithMessage(ObjectUploadValidationMessages.ExtensionsRequired).WithErrorCode(ObjectUploadValidationCodes.ExtensionsRequired)
            .MaximumLength(40).WithMessage(ObjectUploadValidationMessages.ExtensionsMaxLength).WithErrorCode(ObjectUploadValidationCodes.ExtensionsMaxLength);
        RuleFor(x => x.FileName).NotEmpty().WithMessage(ObjectUploadValidationMessages.FileNameRequired).WithErrorCode(ObjectUploadValidationCodes.FileNameRequired)
            .MaximumLength(80).WithMessage(ObjectUploadValidationMessages.FileNameMaxLength).WithErrorCode(ObjectUploadValidationCodes.FileNameMaxLength);
        RuleFor(x => x.MaxLength).NotNull().WithMessage(ObjectUploadValidationMessages.MaxLengthRequired).WithErrorCode(ObjectUploadValidationCodes.MaxLengthRequired)
            .GreaterThan(0).WithMessage(ObjectUploadValidationMessages.MaxLengthPositive).WithErrorCode(ObjectUploadValidationCodes.MaxLengthPositive);
        RuleFor(x => x.Expiration).NotNull().WithMessage(ObjectUploadValidationMessages.ExpirationRequired).WithErrorCode(ObjectUploadValidationCodes.ExpirationRequired)
            .GreaterThan(0).WithMessage(ObjectUploadValidationMessages.ExpirationPositive).WithErrorCode(ObjectUploadValidationCodes.ExpirationPositive);
    }
}
