using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;

/// <summary>Acota la consulta y exige un ámbito interno.</summary>
public sealed class GetImportProgressQueryValidator : AbstractValidator<GetImportProgressQuery>
{
    /// <summary>Rechaza listas vacías, repetidas o demasiado grandes antes de acceder a persistencia.</summary>
    public GetImportProgressQueryValidator()
    {
        RuleFor(x => x.Requester).NotNull();
        RuleFor(x => x.LoadCodes)
            .NotNull()
            .Must(x =>
                x is { Length: > 0 and <= 50 }
                && x.All(id => id != Guid.Empty)
                && x.Distinct().Count() == x.Length
            )
            .WithErrorCode("IMPORT_PROGRESS_CODES_INVALID")
            .WithMessage("Selecciona hasta cincuenta cargas distintas.");
    }
}
