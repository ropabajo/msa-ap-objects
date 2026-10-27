using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.GetFileDownload;

/// <summary>
/// Entrada HTTP del caso GetFileDownload; la ruta y el contexto mantienen su autoridad.
/// </summary>
[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed class GetFileDownloadRequestDto
{
    /// <summary>
    /// Código de la iglesia autorizado desde la ruta.
    /// </summary>
    /// <example>8cbddf7f-a8ab-46a0-a832-373f454357cd</example>
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid ChurchCode { get; set; }

    /// <summary>
    /// Identificador del trabajo desde la ruta.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid JobId { get; set; }

    /// <summary>
    /// Identificador del archivo autorizado para descargar.
    /// </summary>
    /// <example>6d2e9eb4-24d6-4335-8297-29968401bdf7</example>
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid ArtifactId { get; set; }
}
