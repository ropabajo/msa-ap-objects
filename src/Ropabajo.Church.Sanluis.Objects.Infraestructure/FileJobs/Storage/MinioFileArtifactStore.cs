using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.MinIo;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Storage;

/// <summary>
/// Almacena y firma solo artefactos reservados bajo el prefijo de trabajos de archivos.
/// </summary>
public sealed class MinioFileArtifactStore : IFileArtifactStore, IDisposable
{
    private readonly MinIoOptions _options;
    private readonly Lazy<IMinioClient> _client;

    /// <summary>
    /// Inicializa el cliente diferido con la identidad privada de Objects.
    /// </summary>
    public MinioFileArtifactStore(IOptions<MinIoOptions> options)
    {
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        _client = new(() => new MinioClient().WithEndpoint(_options.Endpoint).WithCredentials(_options.AccessKey, _options.SecretKey).WithSSL(_options.UseSsl).Build());
    }

    /// <inheritdoc />
    public async Task<FileUploadGrant> GrantAsync(FileJobArtifact artifact, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        Validate(artifact); cancellationToken.ThrowIfCancellationRequested();
        var policy = new PostPolicy();
        policy.SetBucket(_options.BucketName); policy.SetKey(artifact.StorageKey); policy.SetContentType(artifact.ContentType);
        policy.SetContentRange(artifact.SizeBytes, artifact.SizeBytes); policy.SetExpires(expiresAt.UtcDateTime);
        var grant = await _client.Value.PresignedPostPolicyAsync(new PresignedPostPolicyArgs().WithBucket(_options.BucketName).WithObject(artifact.StorageKey).WithPolicy(policy)).WaitAsync(cancellationToken);
        return new FileUploadGrant { ArtifactId = artifact.ArtifactId, Url = grant.Item1.AbsoluteUri, Fields = new Dictionary<string, string>(grant.Item2), ExpiresAt = expiresAt };
    }

    /// <inheritdoc />
    public async Task<bool> VerifyAsync(FileJobArtifact artifact, CancellationToken cancellationToken)
    {
        Validate(artifact);
        try
        {
            var stat = await _client.Value.StatObjectAsync(new StatObjectArgs().WithBucket(_options.BucketName).WithObject(artifact.StorageKey), cancellationToken);
            if (stat.Size != artifact.SizeBytes || !string.Equals(stat.ContentType, artifact.ContentType, StringComparison.OrdinalIgnoreCase)) return false;
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            long count = 0;
            await _client.Value.GetObjectAsync(new GetObjectArgs().WithBucket(_options.BucketName).WithObject(artifact.StorageKey)
                .WithCallbackStream(async (input, token) =>
                {
                    var buffer = new byte[65536]; int read;
                    while ((read = await input.ReadAsync(buffer, token)) > 0)
                    {
                        count += read;
                        if (count > artifact.SizeBytes) throw new InvalidDataException("File artifact exceeds its reserved size.");
                        hash.AppendData(buffer, 0, read);
                    }
                }), cancellationToken);
            return count == artifact.SizeBytes && Convert.ToHexString(hash.GetHashAndReset()).Equals(artifact.Sha256, StringComparison.OrdinalIgnoreCase);
        }
        catch (ObjectNotFoundException) { return false; }
    }

    /// <inheritdoc />
    public async Task<Result<FileDownload>> SignAsync(FileJobArtifact artifact, DateTimeOffset issuedAt, DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        Validate(artifact); cancellationToken.ThrowIfCancellationRequested();
        var seconds = (int)Math.Floor((expiresAt - issuedAt).TotalSeconds);
        if (seconds < 1 || seconds > 3600) throw new InvalidOperationException("File signature lifetime is invalid.");
        try { await _client.Value.StatObjectAsync(new StatObjectArgs().WithBucket(_options.BucketName).WithObject(artifact.StorageKey), cancellationToken); }
        catch (ObjectNotFoundException) { return Result.Failure<FileDownload>(new Error("FILE_ARTIFACT_MISSING", "El archivo ya no está disponible. Solicita una exportación nueva.", ErrorType.NotFound)); }

        var url = await _client.Value.PresignedGetObjectAsync(new PresignedGetObjectArgs().WithBucket(_options.BucketName).WithObject(artifact.StorageKey)
            .WithExpiry(seconds).WithRequestDate(issuedAt.UtcDateTime).WithHeaders(new Dictionary<string, string>
            {
                ["response-content-type"] = artifact.ContentType,
                ["response-content-disposition"] = "attachment; filename=" + artifact.FileName
            })).WaitAsync(cancellationToken);
        return new FileDownload { Url = url, FileName = artifact.FileName, ExpiresAt = issuedAt.AddSeconds(seconds) };
    }

    /// <inheritdoc />
    public Task DeleteAsync(FileJobArtifact artifact, CancellationToken cancellationToken)
    {
        Validate(artifact);
        return _client.Value.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_options.BucketName).WithObject(artifact.StorageKey), cancellationToken);
    }

    private static void Validate(FileJobArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        if (!artifact.StorageKey.StartsWith("file-jobs/v1/", StringComparison.Ordinal) || artifact.StorageKey.Contains("..") || artifact.StorageKey.Contains('\\')
            || artifact.FileName.IndexOfAny(['/', '\\', '\r', '\n', '"']) >= 0)
            throw new InvalidOperationException("File artifact key or filename is outside its reserved scope.");
    }

    /// <summary>
    /// Libera conexiones del cliente cuando termina el host.
    /// </summary>
    public void Dispose() { if (_client.IsValueCreated) _client.Value.Dispose(); }
}
