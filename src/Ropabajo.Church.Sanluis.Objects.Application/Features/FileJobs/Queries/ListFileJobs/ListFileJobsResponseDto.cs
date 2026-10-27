using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.ListFileJobs;

/// <summary>
/// Contiene una página de trabajos autorizados y un cursor para continuar.
/// </summary>
public sealed class ListFileJobsResponseDto
{
    /// <summary>
    /// Trabajos de la página actual.
    /// </summary>
    public IReadOnlyList<FileJobSummary> Items { get; set; } = Array.Empty<FileJobSummary>();

    /// <summary>
    /// Cursor opaco para obtener la siguiente página.
    /// </summary>
    /// <example>MTI1</example>
    public string? NextCursor { get; set; }
}
