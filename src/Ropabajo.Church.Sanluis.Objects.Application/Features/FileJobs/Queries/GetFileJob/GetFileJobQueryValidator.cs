using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.GetFileJob;

/// <summary>
/// Valida las entradas del caso GetFileJob.
/// </summary>
public sealed class GetFileJobQueryValidator : AbstractValidator<GetFileJobQuery>
{
    /// <summary>
    /// Inicializa las restricciones del contrato.
    /// </summary>
    public GetFileJobQueryValidator()
    {
        RuleFor(query => query.ChurchCode).NotEmpty()
            .WithMessage(FileJobValidationMessages.ChurchCodeRequired).WithErrorCode(FileJobValidationCodes.ChurchCodeRequired);
        RuleFor(query => query.JobId).NotEmpty()
            .WithMessage(FileJobValidationMessages.JobIdRequired).WithErrorCode(FileJobValidationCodes.JobIdRequired);
    }
}
