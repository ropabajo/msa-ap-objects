namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Obtiene exclusivamente la identidad y autorización verificadas de la solicitud actual.
/// </summary>
public interface IImportActor
{
    /// <summary>
    /// Obtiene un solicitante autorizado para la iglesia de la ruta.
    /// </summary>
    ImportRequester GetRequester(Guid churchCode);
}
