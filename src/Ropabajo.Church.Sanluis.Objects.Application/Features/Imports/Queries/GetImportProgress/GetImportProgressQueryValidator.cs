using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;

/// <summary>
/// Acota la consulta y exige un ámbito interno.
/// </summary>
public sealed class GetImportProgressQueryValidator : AbstractValidator<GetImportProgressQuery>
{
    /// <summary>
    /// Rechaza listas vacías, repetidas o demasiado grandes antes de acceder a persistencia.
    /// </summary>
    public GetImportProgressQueryValidator()
    {
        RuleFor(x => x.Requester)
            .NotNull()
            .WithErrorCode(ImportValidationCodes.RequesterRequired)
            .WithMessage(ImportValidationMessages.RequesterRequired);
        RuleFor(x => x.LoadCodes)
            .Must(x =>
                x is { Length: > 0 and <= 100 }
                && x.All(id => id != Guid.Empty)
                && x.Distinct().Count() == x.Length
            )
            .WithErrorCode(ImportValidationCodes.LoadCodesValid)
            .WithMessage(ImportValidationMessages.LoadCodesValid);
    }
}
