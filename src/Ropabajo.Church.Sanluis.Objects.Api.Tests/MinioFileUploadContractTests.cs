using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.Files.Configuration;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.Files.Workers;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Storage;
using Xunit;

public sealed class MinioFileUploadContractTests
{
    [Fact]
    public async Task Grant_and_worker_send_the_mime_field_required_by_the_signed_post_policy()
    {
        const string mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var bytes = Encoding.UTF8.GetBytes("Synthetic isolated upload fixture");
        var accepted = false;
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        await using var server = builder.Build();
        server.Run(async context =>
        {
            if (context.Request.Method == "GET")
            {
                context.Response.ContentType = "application/xml";
                await context.Response.WriteAsync("<LocationConstraint xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\">us-east-1</LocationConstraint>");
                return;
            }
            var form = await context.Request.ReadFormAsync();
            using var policy = JsonDocument.Parse(Convert.FromBase64String(form["policy"]!));
            var mimeRequired = policy.RootElement.GetProperty("conditions").EnumerateArray().Any(condition =>
                condition.ValueKind == JsonValueKind.Array && condition.GetArrayLength() == 3
                && condition[0].GetString() == "eq" && condition[1].GetString() == "$Content-Type" && condition[2].GetString() == mime);
            var file = form.Files.GetFile("file");
            using var received = new MemoryStream();
            if (file is not null) await file.CopyToAsync(received);
            accepted = mimeRequired && form["Content-Type"] == mime && file?.ContentType == mime
                && bytes.SequenceEqual(received.ToArray());
            context.Response.StatusCode = accepted ? StatusCodes.Status204NoContent : StatusCodes.Status403Forbidden;
        });
        await server.StartAsync();
        var endpoint = new Uri(server.Urls.Single());
        using var store = new MinioFileArtifactStore(Options.Create(new MinIoOptions
        {
            Endpoint = endpoint.Authority, UseSsl = false, BucketName = "fixture-bucket",
            AccessKey = "fixture-access", SecretKey = "fixture-secret-not-real"
        }));
        var artifact = new FileJobArtifact
        {
            ArtifactId = Guid.NewGuid(), Name = "workbook", FileName = "fixture.xlsx",
            StorageKey = "file-jobs/v1/development/fixture.xlsx", ContentType = mime,
            SizeBytes = bytes.LongLength, Sha256 = Convert.ToHexString(SHA256.HashData(bytes))
        };
        var path = Path.Combine(Path.GetTempPath(), "minio-upload-" + Guid.NewGuid().ToString("N"));
        try
        {
            await File.WriteAllBytesAsync(path, bytes);
            var grant = await store.GrantAsync(artifact, DateTimeOffset.UtcNow.AddMinutes(1), default);
            using var http = new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false });
            var worker = new FileWorkerHttpClient(http, http, http,
                Options.Create(new FileWorkerSettings { StorageHosts = [IPAddress.Loopback.ToString()] }), TimeProvider.System);
            var result = await worker.UploadAsync(grant, new GeneratedFile
            {
                Name = artifact.Name, Path = path, SizeBytes = artifact.SizeBytes, Sha256 = artifact.Sha256
            }, default);
            Assert.True(result.IsSuccess);
            Assert.True(accepted);
        }
        finally { File.Delete(path); }
    }
}
