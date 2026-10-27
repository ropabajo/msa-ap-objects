using System.ComponentModel.DataAnnotations;
using Ropabajo.Church.Sanluis.Objects.Application.Constants;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CreateFileJob;

/// <summary>
/// Entrada HTTP del caso CreateFileJob; la ruta y el contexto mantienen su autoridad.
/// </summary>
[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateFileJobRequestDto
{
    /// <summary>
    /// Código de la iglesia autorizado desde la ruta.
    /// </summary>
    /// <example>8cbddf7f-a8ab-46a0-a832-373f454357cd</example>
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid ChurchCode { get; set; }

    /// <summary>
    /// Identificador de intención que se conserva ante respuestas inciertas.
    /// </summary>
    /// <example>3152bb49-e1aa-4df5-b333-4f96f8b0fc12</example>
    [Required(ErrorMessage = FileJobValidationMessages.RequestIdRequired)]
    public Guid RequestId { get; set; }

    /// <summary>
    /// Tipo de reporte permitido por el catálogo.
    /// </summary>
    /// <example>members.excel</example>
    [Required(ErrorMessage = FileJobValidationMessages.ReportTypeRequired)]
    [MaxLength(100, ErrorMessage = FileJobValidationMessages.ReportTypeMaxLength)]
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Versión del contrato de parámetros del reporte.
    /// </summary>
    /// <example>1</example>
    [Range(1, 999, ErrorMessage = FileJobValidationMessages.ContractVersionRange)]
    public int ContractVersion { get; set; } = 1;

    /// <summary>
    /// Parámetros pequeños del reporte sin listas de registros.
    /// </summary>
    public System.Text.Json.JsonElement Parameters { get; set; }

    /// <summary>
    /// Solicita datos nuevos cuando no existe una generación equivalente activa.
    /// </summary>
    /// <example>false</example>
    public bool ForceRefresh { get; set; }
}
