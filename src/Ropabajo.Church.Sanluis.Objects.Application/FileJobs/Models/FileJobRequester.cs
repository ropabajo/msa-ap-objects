namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

/// <summary>
/// Conserva el ámbito del usuario obtenido de autenticación y autorización vigentes.
/// </summary>
/// <param name="ChurchCode">Iglesia autorizada por el servidor.</param>
/// <param name="OwnerHash">Hash del emisor y sujeto autenticados.</param>
/// <param name="Roles">Permisos efectivos del ámbito, nunca recibidos del body.</param>
public sealed record FileJobRequester(Guid ChurchCode, string OwnerHash, IReadOnlyCollection<string> Roles);
