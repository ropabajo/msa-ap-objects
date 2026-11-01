using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;

/// <summary>
/// Instantánea de contadores confirmados dentro del ámbito autorizado.
/// </summary>
public sealed class GetImportProgressResponseDto
{
    /// <summary>
    /// Avance y versión de las cargas autorizadas.
    /// </summary>
    public IReadOnlyList<ImportProgressSnapshot> Items { get; set; } = [];
}
