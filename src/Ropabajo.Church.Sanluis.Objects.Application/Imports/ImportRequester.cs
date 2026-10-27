namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Identidad autenticada y ámbito autorizado para el registro de importaciones.
/// </summary>
/// <param name="ChurchCode">Iglesia autorizada por IAM para la ruta.</param>
/// <param name="OwnerHash">Huella del emisor y sujeto autenticados.</param>
/// <param name="UserId">Sujeto autenticado usado en auditoría.</param>
/// <param name="Username">Nombre de cuenta verificado para enlazar el objeto subido.</param>
/// <param name="Ip">Dirección de la solicitud para auditoría.</param>
public sealed record ImportRequester(Guid ChurchCode, string OwnerHash, string UserId, string Username, string Ip);
