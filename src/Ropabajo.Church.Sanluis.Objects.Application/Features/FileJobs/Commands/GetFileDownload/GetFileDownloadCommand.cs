using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.GetFileDownload;

/// <summary>
/// Ejecuta el caso de uso GetFileDownload dentro del ámbito autorizado.
/// </summary>
public sealed class GetFileDownloadCommand : IRequest<Result<GetFileDownloadResponseDto>>
{
    /// <summary>
    /// Código de la iglesia autorizado desde la ruta.
    /// </summary>
    /// <example>8cbddf7f-a8ab-46a0-a832-373f454357cd</example>
    public Guid ChurchCode { get; set; }

    /// <summary>
    /// Identificador del trabajo desde la ruta.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }

    /// <summary>
    /// Identificador del archivo autorizado para descargar.
    /// </summary>
    /// <example>6d2e9eb4-24d6-4335-8297-29968401bdf7</example>
    public Guid ArtifactId { get; set; }
}
