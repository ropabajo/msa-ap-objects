using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.HeartbeatFileJob;

/// <summary>
/// Valida las entradas del caso HeartbeatFileJob.
/// </summary>
public sealed class HeartbeatFileJobCommandValidator : AbstractValidator<HeartbeatFileJobCommand>
{
    /// <summary>
    /// Inicializa las restricciones del contrato.
    /// </summary>
    public HeartbeatFileJobCommandValidator()
    {
        RuleFor(command => command.JobId).NotEmpty()
            .WithMessage(FileJobValidationMessages.JobIdRequired).WithErrorCode(FileJobValidationCodes.JobIdRequired);
        RuleFor(command => command.AttemptId).NotEmpty()
            .WithMessage(FileJobValidationMessages.AttemptIdRequired).WithErrorCode(FileJobValidationCodes.AttemptIdRequired);
        RuleFor(command => command.LeaseToken).NotEmpty()
            .WithMessage(FileJobValidationMessages.LeaseTokenRequired).WithErrorCode(FileJobValidationCodes.LeaseTokenRequired);
        RuleFor(command => command.LeaseToken).MaximumLength(64).WithMessage(FileJobValidationMessages.LeaseTokenMaxLength).WithErrorCode(FileJobValidationCodes.LeaseTokenMaxLength);
    }
}
