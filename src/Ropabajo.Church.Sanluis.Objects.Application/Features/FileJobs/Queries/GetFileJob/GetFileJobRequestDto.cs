using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.GetFileJob;

/// <summary>
/// Entrada HTTP del caso GetFileJob; la ruta y el contexto mantienen su autoridad.
/// </summary>
[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed class GetFileJobRequestDto
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
}
