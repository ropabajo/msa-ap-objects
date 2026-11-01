using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.GetFileDownload;

/// <summary>
/// Entrega una firma corta para un archivo ya publicado sin regenerarlo.
/// </summary>
public sealed class GetFileDownloadResponseDto
{
    /// <summary>
    /// URL temporal de descarga; no debe registrarse en logs.
    /// </summary>
    /// <example>https://minio.example.com/church-dev/file.xlsx</example>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Nombre seguro del archivo descargado.
    /// </summary>
    /// <example>miembros-20261025.xlsx</example>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Vencimiento UTC de la firma, limitado por la disponibilidad del archivo.
    /// </summary>
    /// <example>2026-10-25T12:15:00Z</example>
    public DateTimeOffset ExpiresAt { get; set; }
}
