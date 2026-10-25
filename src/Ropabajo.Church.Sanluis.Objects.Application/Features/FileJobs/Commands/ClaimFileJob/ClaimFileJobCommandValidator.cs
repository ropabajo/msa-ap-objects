using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ClaimFileJob;

/// <summary>
/// Valida las entradas del caso ClaimFileJob.
/// </summary>
public sealed class ClaimFileJobCommandValidator : AbstractValidator<ClaimFileJobCommand>
{
    /// <summary>
    /// Inicializa las restricciones del contrato.
    /// </summary>
    public ClaimFileJobCommandValidator()
    {
        RuleFor(command => command.JobId).NotEmpty()
            .WithMessage(FileJobValidationMessages.JobIdRequired).WithErrorCode(FileJobValidationCodes.JobIdRequired);
    }
}
