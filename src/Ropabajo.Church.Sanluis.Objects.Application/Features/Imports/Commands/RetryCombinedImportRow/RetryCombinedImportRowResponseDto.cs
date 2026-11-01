namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.RetryCombinedImportRow;

/// <summary>
/// Resultado propio del caso RetryCombinedImportRow.
/// </summary>
public sealed class RetryCombinedImportRowResponseDto
{
    /// <summary>
    /// Indica que la recuperación se registró para procesarse.
    /// </summary>
    /// <example>true</example>
    public bool Accepted { get; set; }
}
