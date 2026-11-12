using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Photos;
namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Photos;

/// <summary>

/// Reutiliza la identidad MinIO de Objects; valida la foto almacenada antes de confirmar su estado.

/// </summary>
public sealed class MinioObjectUploadStore : IObjectUploadStore, IDisposable
{
    private readonly MinIoOptions _options;
    private readonly Lazy<IMinioClient> _client;
    private readonly SemaphoreSlim _photoGate = new(1, 1);
    /// <summary>
    /// Construye el cliente diferido con la configuración existente, sin hacer I/O en el constructor.
    /// </summary>
    public MinioObjectUploadStore(IOptions<MinIoOptions> options)
    {
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        _client = new(() => new MinioClient().WithEndpoint(_options.Endpoint).WithCredentials(_options.AccessKey, _options.SecretKey).WithSSL(_options.UseSsl).Build());
    }
    /// <inheritdoc />
    public async Task<Result<ObjectUploadGrant>> GrantAsync(string key, string contentType, long maxBytes, int expirationSeconds, CancellationToken cancellationToken)
    {
        try
        {
            var policy = new PostPolicy();
            policy.SetBucket(_options.BucketName); policy.SetKey(key); policy.SetContentType(contentType);
            policy.SetContentRange(1, maxBytes); policy.SetCacheControl("max-age=86400,s-maxage=86400,proxy-revalidate");
            policy.SetExpires(DateTime.UtcNow.AddSeconds(expirationSeconds));
            var grant = await _client.Value.PresignedPostPolicyAsync(new PresignedPostPolicyArgs().WithBucket(_options.BucketName).WithObject(key).WithPolicy(policy)).WaitAsync(cancellationToken);
            var headers = new Dictionary<string, string> { ["url"] = grant.Item1.AbsoluteUri,
                ["x-form-data-content-type"] = contentType, ["x-form-data-cache-control"] = "max-age=86400,s-maxage=86400,proxy-revalidate" };
            foreach (var pair in grant.Item2) headers["x-form-data-" + pair.Key] = pair.Value;
            return new ObjectUploadGrant(headers);
        }
        catch (Exception error) when (error is MinioException or HttpRequestException)
        { return Result.Failure<ObjectUploadGrant>(PhotoUploadErrors.Unavailable); }
    }
    /// <inheritdoc />
    public async Task<Result<VerifiedPhoto>> VerifyPhotoAsync(string key, CancellationToken cancellationToken)
    {
        await _photoGate.WaitAsync(cancellationToken);
        try
        {
            var stat = await _client.Value.StatObjectAsync(new StatObjectArgs().WithBucket(_options.BucketName).WithObject(key), cancellationToken);
            if (stat.Size == 0) return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Empty);
            if (stat.Size > PhotoUploadPolicy.MaxBytes) return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Size);
            using var memory = new MemoryStream((int)stat.Size);
            await _client.Value.GetObjectAsync(new GetObjectArgs().WithBucket(_options.BucketName).WithObject(key).WithMatchETag(stat.ETag)
                .WithCallbackStream(async (input, token) =>
                {
                    var buffer = new byte[65536]; int read;
                    while ((read = await input.ReadAsync(buffer, token)) > 0)
                    {
                        if (memory.Length + read > PhotoUploadPolicy.MaxBytes) throw new InvalidDataException();
                        await memory.WriteAsync(buffer.AsMemory(0, read), token);
                    }
                }), cancellationToken);
            if (memory.Length != stat.Size) return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Invalid);
            return PhotoImageDecoder.Validate(memory.ToArray(), key, stat.ContentType ?? "", cancellationToken);
        }
        catch (ObjectNotFoundException) { return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Missing); }
        catch (InvalidDataException) { return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Size); }
        catch (Exception error) when (error is MinioException or HttpRequestException)
        { return Result.Failure<VerifiedPhoto>(PhotoUploadErrors.Unavailable); }
        finally { _photoGate.Release(); }
    }
    /// <inheritdoc />
    public void Dispose() { if (_client.IsValueCreated) _client.Value.Dispose(); _photoGate.Dispose(); }
}
