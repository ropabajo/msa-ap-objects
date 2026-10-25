using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CancelFileJob;

/// <summary>
/// Valida las entradas del caso CancelFileJob.
/// </summary>
public sealed class CancelFileJobCommandValidator : AbstractValidator<CancelFileJobCommand>
{
    /// <summary>
    /// Inicializa las restricciones del contrato.
    /// </summary>
    public CancelFileJobCommandValidator()
    {
        RuleFor(command => command.ChurchCode).NotEmpty()
            .WithMessage(FileJobValidationMessages.ChurchCodeRequired).WithErrorCode(FileJobValidationCodes.ChurchCodeRequired);
        RuleFor(command => command.JobId).NotEmpty()
            .WithMessage(FileJobValidationMessages.JobIdRequired).WithErrorCode(FileJobValidationCodes.JobIdRequired);
    }
}
