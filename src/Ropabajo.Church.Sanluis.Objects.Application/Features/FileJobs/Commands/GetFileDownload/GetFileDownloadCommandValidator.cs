using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.GetFileDownload;

/// <summary>
/// Valida las entradas del caso GetFileDownload.
/// </summary>
public sealed class GetFileDownloadCommandValidator : AbstractValidator<GetFileDownloadCommand>
{
    /// <summary>
    /// Inicializa las restricciones del contrato.
    /// </summary>
    public GetFileDownloadCommandValidator()
    {
        RuleFor(command => command.ChurchCode).NotEmpty()
            .WithMessage(FileJobValidationMessages.ChurchCodeRequired).WithErrorCode(FileJobValidationCodes.ChurchCodeRequired);
        RuleFor(command => command.JobId).NotEmpty()
            .WithMessage(FileJobValidationMessages.JobIdRequired).WithErrorCode(FileJobValidationCodes.JobIdRequired);
        RuleFor(command => command.ArtifactId).NotEmpty()
            .WithMessage(FileJobValidationMessages.ArtifactIdRequired).WithErrorCode(FileJobValidationCodes.ArtifactIdRequired);
    }
}
