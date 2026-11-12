using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Church.Sanluis.Objects.Application.Photos;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Photos;
using SkiaSharp;
using Xunit;

namespace Ropabajo.Church.Sanluis.Objects.Api.Tests;

public sealed class PhotoStorageContractTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task The_real_minio_client_reads_and_decodes_the_stored_image_and_refuses_oversized_metadata(bool oversized, bool corrupt)
    {
        var bytes = corrupt ? "not a jpeg"u8.ToArray() : PhotoImageValidationTests.Image(SKEncodedImageFormat.Jpeg);
        var objectReads = 0;
        var builder = WebApplication.CreateBuilder(); builder.WebHost.UseUrls("http://127.0.0.1:0"); builder.Logging.ClearProviders();
        await using var app = builder.Build();
        app.Run(async context =>
        {
            if (context.Request.Query.ContainsKey("location"))
            { context.Response.ContentType = "application/xml"; await context.Response.WriteAsync("<LocationConstraint xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">us-east-1</LocationConstraint>"); return; }
            context.Response.ContentType = "image/jpeg";
            context.Response.Headers.ETag = "\"fixture-etag\"";
            context.Response.Headers.LastModified = DateTimeOffset.UtcNow.ToString("R");
            context.Response.ContentLength = oversized ? PhotoUploadPolicy.MaxBytes + 1L : bytes.Length;
            if (context.Request.Method == "GET") { objectReads++; await context.Response.Body.WriteAsync(bytes); }
        });
        await app.StartAsync(); var endpoint = new Uri(app.Urls.Single());
        using var store = new MinioObjectUploadStore(Options.Create(new MinIoOptions { Endpoint = endpoint.Authority, UseSsl = false,
            BucketName = "fixture-bucket", AccessKey = "fixture-access", SecretKey = "fixture-secret-not-real" }));
        var result = await store.VerifyPhotoAsync("photos/11111111-1111-4111-8111-111111111111.jpeg", default);
        Assert.Equal(!oversized && !corrupt, result.IsSuccess);
        if (oversized) { Assert.Equal(0, objectReads); Assert.Equal("PHOTO_FILE_SIZE", Assert.Single(result.Errors).Code); }
        else Assert.True(objectReads > 0);
        if (corrupt) Assert.Equal("PHOTO_FILE_INVALID", Assert.Single(result.Errors).Code);
    }

    [Fact]
    public async Task Signed_photo_policy_contains_the_server_size_bound_and_the_required_mime_form_field()
    {
        var builder = WebApplication.CreateBuilder(); builder.WebHost.UseUrls("http://127.0.0.1:0"); builder.Logging.ClearProviders();
        await using var app = builder.Build();
        app.Run(async context => { context.Response.ContentType = "application/xml";
            await context.Response.WriteAsync("<LocationConstraint xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">us-east-1</LocationConstraint>"); });
        await app.StartAsync(); var endpoint = new Uri(app.Urls.Single());
        using var store = new MinioObjectUploadStore(Options.Create(new MinIoOptions { Endpoint = endpoint.Authority, UseSsl = false,
            BucketName = "fixture-bucket", AccessKey = "fixture-access", SecretKey = "fixture-secret-not-real" }));
        var result = await store.GrantAsync("photos/11111111-1111-4111-8111-111111111111.jpeg", "image/jpeg", PhotoUploadPolicy.MaxBytes, 300, default);
        Assert.True(result.IsSuccess);
        Assert.Equal("image/jpeg", result.Value.Headers["x-form-data-content-type"]);
        using var policy = JsonDocument.Parse(Convert.FromBase64String(result.Value.Headers["x-form-data-policy"]));
        Assert.Contains(policy.RootElement.GetProperty("conditions").EnumerateArray(), value => value.ValueKind == JsonValueKind.Array
            && value[0].GetString() == "content-length-range" && long.Parse(value[1].ToString()) == 1 && long.Parse(value[2].ToString()) == PhotoUploadPolicy.MaxBytes);
    }
}
