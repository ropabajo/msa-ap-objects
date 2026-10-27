using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CompleteFileJob;

/// <summary>
/// Ejecuta el caso de uso CompleteFileJob dentro del ámbito autorizado.
/// </summary>
public sealed class CompleteFileJobCommand : IRequest<Result<CompleteFileJobResponseDto>>
{
    /// <summary>
    /// Identificador del trabajo desde la ruta.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }

    /// <summary>
    /// Identificador del intento desde la ruta.
    /// </summary>
    /// <example>fa8e9db0-c458-48ba-81e2-d54d94655983</example>
    public Guid AttemptId { get; set; }

    /// <summary>
    /// Credencial del intento enlazada desde el encabezado técnico.
    /// </summary>
    /// <example>0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef</example>
    public string LeaseToken { get; set; } = string.Empty;

    /// <summary>
    /// Identificadores de los archivos reservados y subidos.
    /// </summary>
    public IReadOnlyList<Guid> ArtifactIds { get; set; } = Array.Empty<Guid>();

    /// <summary>
    /// Fecha UTC de la lectura consistente del reporte.
    /// </summary>
    /// <example>2026-10-25T12:00:06Z</example>
    public DateTimeOffset DataAsOf { get; set; }
}
