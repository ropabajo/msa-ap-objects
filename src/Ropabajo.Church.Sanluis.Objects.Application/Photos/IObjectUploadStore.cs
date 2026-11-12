using Ropabajo.Churc.Sanluis.Framework.Primitives;
namespace Ropabajo.Church.Sanluis.Objects.Application.Photos;

/// <summary>

/// Autorización firmada conservada en las cabeceras públicas del endpoint existente.

/// </summary>
/// <param name="Headers">Cabeceras que transportan la URL, política y campos del formulario.</param>
public sealed record ObjectUploadGrant(IReadOnlyDictionary<string, string> Headers);

/// <summary>

/// Dimensiones verificadas de una foto almacenada.

/// </summary>
/// <param name="Width">Ancho real en píxeles.</param>
/// <param name="Height">Alto real en píxeles.</param>
public sealed record VerifiedPhoto(int Width, int Height);

/// <summary>

/// Puerto de almacenamiento; la infraestructura firma y comprueba contenido sin abrir transacciones de negocio.

/// </summary>
public interface IObjectUploadStore
{
    /// <summary>
    /// Firma una carga con los límites decididos por Application, sin recibir credenciales del cliente.
    /// </summary>
    Task<Result<ObjectUploadGrant>> GrantAsync(string key, string contentType, long maxBytes, int expirationSeconds, CancellationToken cancellationToken);
    /// <summary>
    /// Comprueba tamaño, MIME, firma y decodificación del objeto antes de confirmar una foto.
    /// </summary>
    Task<Result<VerifiedPhoto>> VerifyPhotoAsync(string key, CancellationToken cancellationToken);
}
