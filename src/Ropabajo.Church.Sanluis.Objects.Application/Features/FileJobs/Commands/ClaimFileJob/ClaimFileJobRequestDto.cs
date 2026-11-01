using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ClaimFileJob;

/// <summary>
/// Entrada HTTP del caso ClaimFileJob; la ruta y el contexto mantienen su autoridad.
/// </summary>
[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed class ClaimFileJobRequestDto
{
    /// <summary>
    /// Identificador del trabajo desde la ruta.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid JobId { get; set; }
}
