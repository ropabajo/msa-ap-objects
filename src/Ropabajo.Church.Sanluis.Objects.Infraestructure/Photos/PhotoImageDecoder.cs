using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Photos;
using SkiaSharp;
namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Photos;

/// <summary>

/// Decodifica fotos con límites de entrada y memoria; sólo admite los formatos acordados.

/// </summary>
public static class PhotoImageDecoder
{
    /// <summary>
    /// Comprueba el archivo completo para el fotograma que se muestra como foto, no sólo su extensión o encabezado.
    /// </summary>
    public static Result<VerifiedPhoto> Validate(byte[] bytes, string key, string contentType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (bytes.Length == 0) return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Empty);
        if (bytes.Length > PhotoUploadPolicy.MaxBytes) return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Size);
        var expected = PhotoUploadPolicy.ContentType(key);
        if (expected is null || !string.Equals(expected, contentType.Split(';')[0].Trim(), StringComparison.OrdinalIgnoreCase))
            return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Invalid);
        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data);
        if (codec is null) return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Invalid);
        var format = codec.EncodedFormat switch { SKEncodedImageFormat.Jpeg => "image/jpeg", SKEncodedImageFormat.Png => "image/png", SKEncodedImageFormat.Webp => "image/webp", _ => "" };
        if (format != expected || codec.Info.Width <= 0 || codec.Info.Height <= 0) return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Invalid);
        if ((long)codec.Info.Width * codec.Info.Height > PhotoUploadPolicy.MaxPixels) return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Dimensions);
        using var bitmap = new SKBitmap(new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
        var decoded = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
        cancellationToken.ThrowIfCancellationRequested();
        return decoded == SKCodecResult.Success ? new VerifiedPhoto(codec.Info.Width, codec.Info.Height)
            : Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Invalid);
    }
}
