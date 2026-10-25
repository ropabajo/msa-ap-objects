using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;

/// <summary>
/// Puerto de objetos limitado a las claves reservadas por el registro de trabajos.
/// </summary>
public interface IFileArtifactStore
{
    /// <summary>
    /// Firma una política POST para el objeto, tipo y tamaño exactos del intento.
    /// </summary>
    Task<FileUploadGrant> GrantAsync(FileJobArtifact artifact, DateTimeOffset expiresAt, CancellationToken cancellationToken);

    /// <summary>
    /// Verifica existencia, tipo, tamaño y SHA-256 mediante lectura acotada por streaming.
    /// </summary>
    Task<bool> VerifyAsync(FileJobArtifact artifact, CancellationToken cancellationToken);

    /// <summary>
    /// Firma la descarga de un archivo verificado hasta la fecha UTC indicada.
    /// </summary>
    Task<Result<FileDownload>> SignAsync(FileJobArtifact artifact, DateTimeOffset issuedAt, DateTimeOffset expiresAt, CancellationToken cancellationToken);

    /// <summary>
    /// Retira un objeto del prefijo de trabajos, sin acceso a fotos ni otros reportes.
    /// </summary>
    Task DeleteAsync(FileJobArtifact artifact, CancellationToken cancellationToken);
}
