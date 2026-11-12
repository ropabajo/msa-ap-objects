using Ropabajo.Churc.Sanluis.Framework.Primitives;
namespace Ropabajo.Church.Sanluis.Objects.Application.Photos;

/// <summary>

/// Fallos seguros de las fotos; no incluyen rutas, nombres internos ni excepciones de almacenamiento.

/// </summary>
public static class PhotoUploadErrors
{
    /// <summary>
    /// El archivo no corresponde a un formato admitido o está dañado.
    /// </summary>
    public static Error Invalid => new("PHOTO_FILE_INVALID", "El archivo no se puede abrir como imagen. Selecciona otra foto JPG/JPEG, PNG o WebP.", ErrorType.Validation);
    /// <summary>
    /// El archivo no tiene contenido.
    /// </summary>
    public static Error Empty => new("PHOTO_FILE_EMPTY", "El archivo está vacío. Selecciona otra imagen.", ErrorType.Validation);
    /// <summary>
    /// El tamaño supera el límite del servidor.
    /// </summary>
    public static Error Size => new("PHOTO_FILE_SIZE", "La foto supera los 5 MB. Reduce su tamaño o selecciona otra imagen.", ErrorType.Validation);
    /// <summary>
    /// La resolución supera el límite acordado.
    /// </summary>
    public static Error Dimensions => new("PHOTO_FILE_DIMENSIONS", "La foto supera los 24 megapíxeles. Reduce sus dimensiones o selecciona otra imagen.", ErrorType.Validation);
    /// <summary>
    /// El almacenamiento no contiene la carga solicitada.
    /// </summary>
    public static Error Missing => new("PHOTO_FILE_MISSING", "No se encontró la imagen cargada. Vuelve a seleccionarla e inténtalo nuevamente.", ErrorType.Validation);
    /// <summary>
    /// El servicio no pudo completar su comprobación.
    /// </summary>
    public static Error Unavailable => new("PHOTO_STORAGE_UNAVAILABLE", "El servicio de fotos no está disponible. Inténtalo más tarde; si continúa, contacta al administrador.", ErrorType.Unavailable);
}
