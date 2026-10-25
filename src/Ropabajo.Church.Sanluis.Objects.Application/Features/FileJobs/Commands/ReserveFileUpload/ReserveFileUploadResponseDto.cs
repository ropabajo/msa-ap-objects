using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ReserveFileUpload;

/// <summary>
/// Autoriza una subida limitada a un objeto del intento actual.
/// </summary>
public sealed class ReserveFileUploadResponseDto
{
    /// <summary>
    /// Identificador reservado para el archivo del intento.
    /// </summary>
    /// <example>6d2e9eb4-24d6-4335-8297-29968401bdf7</example>
    public Guid ArtifactId { get; set; }

    /// <summary>
    /// URL HTTPS de subida directa al almacenamiento autorizado.
    /// </summary>
    /// <example>https://minio.example.com/church-dev</example>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Campos firmados de la política POST, tratados como credenciales temporales.
    /// </summary>
    public IReadOnlyDictionary<string,string> Fields { get; set; } = new Dictionary<string,string>();

    /// <summary>
    /// Vencimiento UTC de la autorización de subida.
    /// </summary>
    /// <example>2026-10-25T12:10:00Z</example>
    public DateTimeOffset ExpiresAt { get; set; }
}
