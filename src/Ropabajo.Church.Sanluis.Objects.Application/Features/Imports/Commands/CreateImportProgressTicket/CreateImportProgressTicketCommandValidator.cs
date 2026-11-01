using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateImportProgressTicket;

/// <summary>
/// Valida el ámbito antes de preparar una conexión.
/// </summary>
public sealed class CreateImportProgressTicketCommandValidator
    : AbstractValidator<CreateImportProgressTicketCommand>
{
    /// <summary>
    /// Exige un código público de iglesia válido.
    /// </summary>
    public CreateImportProgressTicketCommandValidator()
    {
        RuleFor(x => x.ChurchCode)
            .NotEmpty()
            .WithErrorCode(ImportValidationCodes.ChurchCodeRequired)
            .WithMessage(ImportValidationMessages.ChurchCodeRequired);
    }
}
