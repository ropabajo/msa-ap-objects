namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateImportProgressTicket;

/// <summary>
/// Entrega opaca de un uso para iniciar la conexión; no contiene el token de la sesión.
/// </summary>
public sealed class CreateImportProgressTicketResponseDto
{
    /// <summary>
    /// Nonce que caduca en veinte segundos y se consume antes del upgrade.
    /// </summary>
    /// <example>ae8b2d1090749ae2a876dca2d353379cbbab54a02dc75cd588b95411e316391f</example>
    public string Ticket { get; set; } = string.Empty;

    /// <summary>
    /// Fecha UTC máxima para iniciar la conexión.
    /// </summary>
    /// <example>2026-10-28T05:30:20Z</example>
    public DateTimeOffset ExpiresAt { get; set; }
}
