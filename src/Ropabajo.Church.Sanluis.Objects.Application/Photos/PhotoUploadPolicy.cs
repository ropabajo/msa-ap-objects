namespace Ropabajo.Church.Sanluis.Objects.Application.Photos;

/// <summary>
/// Reglas del servidor para fotos de personas, independientes de los límites enviados por el navegador.
/// </summary>
public static class PhotoUploadPolicy
{
    /// <summary>
    /// Máximo de bytes del archivo, equivalente a los 5 MB anunciados en la web.
    /// </summary>
    public const int MaxBytes = 5 * 1024 * 1024;
    /// <summary>
    /// Máximo acordado de píxeles para acotar la decodificación de imágenes comprimidas.
    /// </summary>
    public const long MaxPixels = 24_000_000;
    /// <summary>
    /// Extensiones de imagen admitidas para fotos de personas.
    /// </summary>
    public const string Extensions = ".jpg,.jpeg,.png,.webp";
    /// <summary>
    /// Reconoce la carpeta de fotos sin depender de mayúsculas o una barra inicial.
    /// </summary>
    public static bool IsPhotoPath(string? path) => Uri.UnescapeDataString(path ?? "").Trim('/').Equals("photos", StringComparison.OrdinalIgnoreCase);
    /// <summary>
    /// Obtiene el MIME canónico del formato solicitado o null si no está admitido.
    /// </summary>
    public static string? ContentType(string? name) => Path.GetExtension(name ?? "").ToLowerInvariant() switch
    { ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png", ".webp" => "image/webp", _ => null };
}
