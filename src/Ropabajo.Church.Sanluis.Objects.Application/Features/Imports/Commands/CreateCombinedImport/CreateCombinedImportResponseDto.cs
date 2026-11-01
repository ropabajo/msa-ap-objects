namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateCombinedImport;

/// <summary>
/// Resultado propio del caso CreateCombinedImport.
/// </summary>
public sealed class CreateCombinedImportResponseDto
{
    /// <summary>
    /// Código público de la carga aceptada.
    /// </summary>
    /// <example>8431e4c1-4a16-46ab-8932-e02851c073dd</example>
    public Guid LoadCode { get; set; }

    /// <summary>
    /// Estado actual de la carga; la aceptación no confirma la importación de filas.
    /// </summary>
    /// <example>pending</example>
    public string Status { get; set; } = string.Empty;
}
