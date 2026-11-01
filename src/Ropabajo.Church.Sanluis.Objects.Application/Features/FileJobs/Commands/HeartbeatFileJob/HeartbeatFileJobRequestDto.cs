using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.HeartbeatFileJob;

/// <summary>
/// Entrada HTTP del caso HeartbeatFileJob; la ruta y el contexto mantienen su autoridad.
/// </summary>
[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed class HeartbeatFileJobRequestDto
{
    /// <summary>
    /// Identificador del trabajo desde la ruta.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid JobId { get; set; }

    /// <summary>
    /// Identificador del intento desde la ruta.
    /// </summary>
    /// <example>fa8e9db0-c458-48ba-81e2-d54d94655983</example>
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid AttemptId { get; set; }

    /// <summary>
    /// Credencial del intento enlazada desde el encabezado técnico.
    /// </summary>
    /// <example>0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef</example>
    [System.Text.Json.Serialization.JsonIgnore]
    public string LeaseToken { get; set; } = string.Empty;
}
