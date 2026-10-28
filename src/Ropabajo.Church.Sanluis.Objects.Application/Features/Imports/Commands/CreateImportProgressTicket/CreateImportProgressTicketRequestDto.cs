using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateImportProgressTicket;

/// <summary>
/// Solicita acceso temporal al avance en la iglesia de la ruta autorizada.
/// </summary>
public sealed class CreateImportProgressTicketRequestDto
{
    /// <summary>
    /// Código público de la iglesia; procede exclusivamente de la ruta HTTP.
    /// </summary>
    /// <example>64f46864-aad9-47ca-b499-af5e2384e70a</example>
    [Required(ErrorMessage = ImportValidationMessages.ChurchCodeRequired)]
    public Guid ChurchCode { get; set; }
}
