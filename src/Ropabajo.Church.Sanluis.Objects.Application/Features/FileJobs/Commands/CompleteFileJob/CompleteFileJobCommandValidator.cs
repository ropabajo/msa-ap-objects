using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CompleteFileJob;

/// <summary>
/// Valida las entradas del caso CompleteFileJob.
/// </summary>
public sealed class CompleteFileJobCommandValidator : AbstractValidator<CompleteFileJobCommand>
{
    /// <summary>
    /// Inicializa las restricciones del contrato.
    /// </summary>
    public CompleteFileJobCommandValidator()
    {
        RuleFor(command => command.JobId).NotEmpty()
            .WithMessage(FileJobValidationMessages.JobIdRequired).WithErrorCode(FileJobValidationCodes.JobIdRequired);
        RuleFor(command => command.AttemptId).NotEmpty()
            .WithMessage(FileJobValidationMessages.AttemptIdRequired).WithErrorCode(FileJobValidationCodes.AttemptIdRequired);
        RuleFor(command => command.LeaseToken).NotEmpty()
            .WithMessage(FileJobValidationMessages.LeaseTokenRequired).WithErrorCode(FileJobValidationCodes.LeaseTokenRequired);
        RuleFor(command => command.ArtifactIds).NotEmpty()
            .WithMessage(FileJobValidationMessages.ArtifactIdsRequired).WithErrorCode(FileJobValidationCodes.ArtifactIdsRequired);
        RuleFor(command => command.DataAsOf).NotEmpty()
            .WithMessage(FileJobValidationMessages.DataAsOfRequired).WithErrorCode(FileJobValidationCodes.DataAsOfRequired);
        RuleFor(command => command.LeaseToken).MaximumLength(64).WithMessage(FileJobValidationMessages.LeaseTokenMaxLength).WithErrorCode(FileJobValidationCodes.LeaseTokenMaxLength);
    }
}
