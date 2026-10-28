namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Entrega técnica de un solo uso; no expone el JWT en una URL.
/// </summary>
public sealed record ImportProgressTicket(string Ticket, DateTimeOffset ExpiresAt);
