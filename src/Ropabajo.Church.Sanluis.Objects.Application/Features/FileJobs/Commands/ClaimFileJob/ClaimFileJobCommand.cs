using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ClaimFileJob;

/// <summary>
/// Ejecuta el caso de uso ClaimFileJob dentro del ámbito autorizado.
/// </summary>
public sealed class ClaimFileJobCommand : IRequest<Result<ClaimFileJobResponseDto>>
{
    /// <summary>
    /// Identificador del trabajo desde la ruta.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }
}
