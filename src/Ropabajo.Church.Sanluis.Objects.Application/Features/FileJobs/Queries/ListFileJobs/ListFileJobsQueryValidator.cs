using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.ListFileJobs;

/// <summary>
/// Valida las entradas del caso ListFileJobs.
/// </summary>
public sealed class ListFileJobsQueryValidator : AbstractValidator<ListFileJobsQuery>
{
    /// <summary>
    /// Inicializa las restricciones del contrato.
    /// </summary>
    public ListFileJobsQueryValidator()
    {
        RuleFor(query => query.ChurchCode).NotEmpty()
            .WithMessage(FileJobValidationMessages.ChurchCodeRequired).WithErrorCode(FileJobValidationCodes.ChurchCodeRequired);
        RuleFor(query => query.PageSize).GreaterThan(0)
            .WithMessage(FileJobValidationMessages.PageSizeGtZero).WithErrorCode(FileJobValidationCodes.PageSizeGtZero);
        RuleFor(query => query.Cursor).MaximumLength(128).WithMessage(FileJobValidationMessages.CursorMaxLength).WithErrorCode(FileJobValidationCodes.CursorMaxLength);
        RuleFor(query => query.PageSize).LessThanOrEqualTo(50).WithMessage(FileJobValidationMessages.PageSizeMaximum).WithErrorCode(FileJobValidationCodes.PageSizeMaximum);
    }
}
