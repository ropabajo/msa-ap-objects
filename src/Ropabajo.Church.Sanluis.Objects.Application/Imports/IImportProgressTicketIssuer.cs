namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Adapta la sesión HTTP validada a una conexión de avance autorizada.
/// </summary>
public interface IImportProgressTicketIssuer
{
    /// <summary>
    /// Prepara acceso a los contadores de la identidad y la iglesia ya verificadas.
    /// </summary>
    ImportProgressTicket? Issue(ImportRequester requester);
}
