using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateImportProgressTicket;

/// <summary>Valida el ámbito antes de preparar una conexión.</summary>
public sealed class CreateImportProgressTicketCommandValidator
    : AbstractValidator<CreateImportProgressTicketCommand>
{
    /// <summary>Exige un código público de iglesia válido.</summary>
    public CreateImportProgressTicketCommandValidator()
    {
        RuleFor(x => x.ChurchCode)
            .NotEmpty()
            .WithErrorCode("IMPORT_CHURCH_CODE_REQUIRED")
            .WithMessage("Selecciona una iglesia válida.");
    }
}
