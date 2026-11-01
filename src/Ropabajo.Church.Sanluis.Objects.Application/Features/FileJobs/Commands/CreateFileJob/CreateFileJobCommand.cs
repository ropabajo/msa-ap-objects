using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CreateFileJob;

/// <summary>
/// Ejecuta el caso de uso CreateFileJob dentro del ámbito autorizado.
/// </summary>
public sealed class CreateFileJobCommand : IRequest<Result<CreateFileJobResponseDto>>
{
    /// <summary>
    /// Código de la iglesia autorizado desde la ruta.
    /// </summary>
    /// <example>8cbddf7f-a8ab-46a0-a832-373f454357cd</example>
    public Guid ChurchCode { get; set; }

    /// <summary>
    /// Identificador de intención que se conserva ante respuestas inciertas.
    /// </summary>
    /// <example>3152bb49-e1aa-4df5-b333-4f96f8b0fc12</example>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Tipo de reporte permitido por el catálogo.
    /// </summary>
    /// <example>members.excel</example>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Versión del contrato de parámetros del reporte.
    /// </summary>
    /// <example>1</example>
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
