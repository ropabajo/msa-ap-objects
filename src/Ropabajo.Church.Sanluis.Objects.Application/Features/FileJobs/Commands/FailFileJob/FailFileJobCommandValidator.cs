using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.FailFileJob;

/// <summary>
/// Valida las entradas del caso FailFileJob.
/// </summary>
public sealed class FailFileJobCommandValidator : AbstractValidator<FailFileJobCommand>
{
    /// <summary>
    /// Inicializa las restricciones del contrato.
    /// </summary>
    public FailFileJobCommandValidator()
    {
        RuleFor(command => command.JobId).NotEmpty()
            .WithMessage(FileJobValidationMessages.JobIdRequired).WithErrorCode(FileJobValidationCodes.JobIdRequired);
        RuleFor(command => command.AttemptId).NotEmpty()
            .WithMessage(FileJobValidationMessages.AttemptIdRequired).WithErrorCode(FileJobValidationCodes.AttemptIdRequired);
        RuleFor(command => command.LeaseToken).NotEmpty()
            .WithMessage(FileJobValidationMessages.LeaseTokenRequired).WithErrorCode(FileJobValidationCodes.LeaseTokenRequired);
        RuleFor(command => command.Code).NotEmpty()
            .WithMessage(FileJobValidationMessages.CodeRequired).WithErrorCode(FileJobValidationCodes.CodeRequired);
        RuleFor(command => command.LeaseToken).MaximumLength(64).WithMessage(FileJobValidationMessages.LeaseTokenMaxLength).WithErrorCode(FileJobValidationCodes.LeaseTokenMaxLength);
        RuleFor(command => command.Code).MaximumLength(100).WithMessage(FileJobValidationMessages.CodeMaxLength).WithErrorCode(FileJobValidationCodes.CodeMaxLength);
    }
}
