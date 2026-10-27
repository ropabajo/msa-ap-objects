using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.ListFileJobs;

/// <summary>
/// Ejecuta el caso de uso ListFileJobs dentro del ámbito autorizado.
/// </summary>
public sealed class ListFileJobsQuery : IRequest<Result<ListFileJobsResponseDto>>
{
    /// <summary>
    /// Código de la iglesia autorizado desde la ruta.
    /// </summary>
    /// <example>8cbddf7f-a8ab-46a0-a832-373f454357cd</example>
    public Guid ChurchCode { get; set; }

    /// <summary>
    /// Cursor opaco de continuación de la lista autorizada.
    /// </summary>
    /// <example>MTI1</example>
    public string? Cursor { get; set; }

    /// <summary>
    /// Cantidad máxima de trabajos por página.
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; set; } = 20;
}
