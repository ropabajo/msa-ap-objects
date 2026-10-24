using System.Text;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.TemporaryExports;

/// <summary>Adaptador S3 que restringe todas sus operaciones al prefijo temporal de exportaciones.</summary>
public sealed class MinioExportObjectStore(IOptions<MinIoOptions> options,
    IOptions<TemporaryExportOptions> exports) : IExportObjectStore, IDisposable
{
    private readonly Lazy<IMinioClient> client = new(() => new MinioClient().WithEndpoint(options.Value.Endpoint)
        .WithCredentials(options.Value.AccessKey, options.Value.SecretKey).WithSSL(options.Value.UseSsl).Build());
    private string Bucket => options.Value.BucketName;
    private static void Validate(string key)
    {
        if (!key.StartsWith(TemporaryExportCoordinator.Prefix, StringComparison.Ordinal) || key.Contains("..") || key.Contains('\\'))
            throw new InvalidOperationException("Object key outside temporary export scope.");
    }
    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken token)
    {
        Validate(key);
        try { await client.Value.StatObjectAsync(new StatObjectArgs().WithBucket(Bucket).WithObject(key), token); return true; }
        catch (ObjectNotFoundException) { return false; }
    }
    /// <inheritdoc />
    public async Task<byte[]?> ReadAsync(string key, CancellationToken token)
    {
        if (!await ExistsAsync(key, token)) return null;
        using var content = new MemoryStream();
        await client.Value.GetObjectAsync(new GetObjectArgs().WithBucket(Bucket).WithObject(key)
            .WithCallbackStream(async (input, cancellation) =>
            {
                var buffer = new byte[81920]; int count;
                while ((count = await input.ReadAsync(buffer, cancellation)) > 0)
                {
                    if (content.Length + count > exports.Value.MaxSourceBytes) throw new InvalidDataException("Export object exceeds configured size.");
                    await content.WriteAsync(buffer.AsMemory(0, count), cancellation);
                }
            }), token);
        return content.ToArray();
    }
    /// <inheritdoc />
    public async Task WriteAsync(string key, byte[] content, string contentType, CancellationToken token)
    {
        Validate(key); using var input = new MemoryStream(content, false);
        await client.Value.PutObjectAsync(new PutObjectArgs().WithBucket(Bucket).WithObject(key).WithStreamData(input)
            .WithObjectSize(input.Length).WithContentType(contentType), token);
    }
    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListAsync(string prefix, CancellationToken token)
    {
        Validate(prefix); var keys = new List<string>();
        await foreach (var item in client.Value.ListObjectsEnumAsync(new ListObjectsArgs().WithBucket(Bucket)
            .WithPrefix(prefix).WithRecursive(true), token)) if (!item.IsDir) keys.Add(item.Key);
        return keys;
    }
    /// <inheritdoc />
    public async Task DeleteAsync(string key, CancellationToken token)
    {
        Validate(key); await client.Value.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(Bucket).WithObject(key), token);
    }
    /// <inheritdoc />
    public async Task<string> SignAsync(string key, string fileName, int seconds, DateTimeOffset issuedAt, CancellationToken token)
    {
        Validate(key); token.ThrowIfCancellationRequested();
        return await client.Value.PresignedGetObjectAsync(new PresignedGetObjectArgs().WithBucket(Bucket).WithObject(key)
            .WithExpiry(seconds).WithRequestDate(issuedAt.UtcDateTime).WithHeaders(new Dictionary<string, string>
            {
                ["response-content-type"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ["response-content-disposition"] = "attachment; filename=" + fileName
            }));
    }
    /// <inheritdoc />
    public void Dispose() { if (client.IsValueCreated) client.Value.Dispose(); }
}
