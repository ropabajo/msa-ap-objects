namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateImportProgressTicket;

/// <summary>Solicita acceso temporal al avance en la iglesia de la ruta autorizada.</summary>
public sealed class CreateImportProgressTicketRequestDto
{
    /// <summary>Código público de la iglesia; procede exclusivamente de la ruta HTTP.</summary>
    public Guid ChurchCode { get; set; }
}
