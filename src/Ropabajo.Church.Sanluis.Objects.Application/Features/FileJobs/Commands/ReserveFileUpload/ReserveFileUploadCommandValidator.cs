using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ReserveFileUpload;

/// <summary>
/// Valida las entradas del caso ReserveFileUpload.
/// </summary>
public sealed class ReserveFileUploadCommandValidator : AbstractValidator<ReserveFileUploadCommand>
{
    /// <summary>
    /// Inicializa las restricciones del contrato.
    /// </summary>
    public ReserveFileUploadCommandValidator()
    {
        RuleFor(command => command.JobId).NotEmpty()
            .WithMessage(FileJobValidationMessages.JobIdRequired).WithErrorCode(FileJobValidationCodes.JobIdRequired);
        RuleFor(command => command.AttemptId).NotEmpty()
            .WithMessage(FileJobValidationMessages.AttemptIdRequired).WithErrorCode(FileJobValidationCodes.AttemptIdRequired);
        RuleFor(command => command.LeaseToken).NotEmpty()
            .WithMessage(FileJobValidationMessages.LeaseTokenRequired).WithErrorCode(FileJobValidationCodes.LeaseTokenRequired);
        RuleFor(command => command.Name).NotEmpty()
            .WithMessage(FileJobValidationMessages.NameRequired).WithErrorCode(FileJobValidationCodes.NameRequired);
        RuleFor(command => command.SizeBytes).GreaterThan(0)
            .WithMessage(FileJobValidationMessages.SizeBytesGtZero).WithErrorCode(FileJobValidationCodes.SizeBytesGtZero);
        RuleFor(command => command.Sha256).NotEmpty()
            .WithMessage(FileJobValidationMessages.Sha256Required).WithErrorCode(FileJobValidationCodes.Sha256Required);
        RuleFor(command => command.LeaseToken).MaximumLength(64).WithMessage(FileJobValidationMessages.LeaseTokenMaxLength).WithErrorCode(FileJobValidationCodes.LeaseTokenMaxLength);
        RuleFor(command => command.Name).MaximumLength(50).WithMessage(FileJobValidationMessages.NameMaxLength).WithErrorCode(FileJobValidationCodes.NameMaxLength);
        RuleFor(command => command.Sha256).MaximumLength(64).WithMessage(FileJobValidationMessages.Sha256MaxLength).WithErrorCode(FileJobValidationCodes.Sha256MaxLength);
    }
}
