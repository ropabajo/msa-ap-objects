using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.ListFileJobs;

/// <summary>
/// Entrada HTTP del caso ListFileJobs; la ruta y el contexto mantienen su autoridad.
/// </summary>
[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed class ListFileJobsRequestDto
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
    [MaxLength(128, ErrorMessage = FileJobValidationMessages.CursorMaxLength)]
    public string? Cursor { get; set; }

    /// <summary>
    /// Cantidad máxima de trabajos por página.
    /// </summary>
    /// <example>20</example>
    [Range(1, 50, ErrorMessage = FileJobValidationMessages.PageSizeRange)]
    public int PageSize { get; set; } = 20;
}
