using System.Buffers.Binary;
using Ropabajo.Church.Sanluis.Objects.Application.Photos;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Photos;
using SkiaSharp;
using Xunit;

namespace Ropabajo.Church.Sanluis.Objects.Api.Tests;

public sealed class PhotoImageValidationTests
{
    internal static byte[] Image(SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(2, 2); bitmap.Erase(SKColors.Green);
        using var image = SKImage.FromBitmap(bitmap); using var data = image.Encode(format, 90);
        return data.ToArray();
    }
    [Theory]
    [InlineData("jpg", SKEncodedImageFormat.Jpeg, "image/jpeg")]
    [InlineData("jpeg", SKEncodedImageFormat.Jpeg, "image/jpeg")]
    [InlineData("JPEG", SKEncodedImageFormat.Jpeg, "image/jpeg")]
    [InlineData("png", SKEncodedImageFormat.Png, "image/png")]
    [InlineData("webp", SKEncodedImageFormat.Webp, "image/webp")]
    public void The_real_decoder_accepts_all_announced_formats(string extension, SKEncodedImageFormat format, string mime)
    {
        var result = PhotoImageDecoder.Validate(Image(format), "photos/person." + extension, mime, default);
        Assert.True(result.IsSuccess); Assert.Equal(new VerifiedPhoto(2, 2), result.Value);
    }
    [Fact]
    public void Size_empty_mime_signature_and_truncation_are_checked_independently()
    {
        Assert.Equal("PHOTO_FILE_EMPTY", Error(PhotoImageDecoder.Validate([], "photo.jpg", "image/jpeg", default)));
        Assert.Equal("PHOTO_FILE_SIZE", Error(PhotoImageDecoder.Validate(new byte[PhotoUploadPolicy.MaxBytes + 1], "photo.jpg", "image/jpeg", default)));
        var jpeg = Image(SKEncodedImageFormat.Jpeg);
        Assert.Equal("PHOTO_FILE_INVALID", Error(PhotoImageDecoder.Validate(jpeg, "photo.png", "image/png", default)));
        Assert.Equal("PHOTO_FILE_INVALID", Error(PhotoImageDecoder.Validate(jpeg, "photo.jpg", "application/octet-stream", default)));
        Assert.Equal("PHOTO_FILE_INVALID", Error(PhotoImageDecoder.Validate(jpeg[..(jpeg.Length / 2)], "photo.jpg", "image/jpeg", default)));
        Assert.Equal("PHOTO_FILE_INVALID", Error(PhotoImageDecoder.Validate("not an image"u8.ToArray(), "photo.jpg", "image/jpeg", default)));
    }
    [Fact]
    public void Excessive_dimensions_are_rejected_before_allocating_the_full_bitmap()
    {
        var png = Image(SKEncodedImageFormat.Png);
        BinaryPrimitives.WriteUInt32BigEndian(png.AsSpan(16, 4), 6001);
        BinaryPrimitives.WriteUInt32BigEndian(png.AsSpan(20, 4), 4000);
        uint crc = 0xffffffff;
        foreach (var value in png.AsSpan(12, 17))
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) == 1 ? 0xedb88320u : 0);
        }
        BinaryPrimitives.WriteUInt32BigEndian(png.AsSpan(29, 4), crc ^ 0xffffffff);
        Assert.Equal("PHOTO_FILE_DIMENSIONS", Error(PhotoImageDecoder.Validate(png, "photo.png", "image/png", default)));
    }
    private static string Error(Ropabajo.Churc.Sanluis.Framework.Primitives.Result<VerifiedPhoto> result)
    { Assert.True(result.IsFailure); return Assert.Single(result.Errors).Code; }
}
