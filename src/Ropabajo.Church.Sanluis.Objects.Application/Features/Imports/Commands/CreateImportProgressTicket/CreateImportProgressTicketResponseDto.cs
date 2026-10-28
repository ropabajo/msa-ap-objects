namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateImportProgressTicket;

/// <summary>Entrega opaca de un uso para iniciar la conexión; no contiene el token de la sesión.</summary>
public sealed class CreateImportProgressTicketResponseDto
{
    /// <summary>Nonce que caduca en veinte segundos y se consume antes del upgrade.</summary>
    public string Ticket { get; set; } = string.Empty;

    /// <summary>Fecha UTC máxima para iniciar la conexión.</summary>
    public DateTimeOffset ExpiresAt { get; set; }
}
