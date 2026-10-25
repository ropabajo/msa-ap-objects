using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.GetFileJob;

/// <summary>
/// Expone el estado de un trabajo sin parámetros privados ni ubicaciones internas.
/// </summary>
public sealed class GetFileJobResponseDto
{
    /// <summary>
    /// Identificador estable del trabajo de archivos.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }

    /// <summary>
    /// Tipo de reporte solicitado.
    /// </summary>
    /// <example>members.excel</example>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Estado técnico actual del trabajo.
    /// </summary>
    /// <example>queued</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Fecha UTC de aceptación durable de la solicitud.
    /// </summary>
    /// <example>2026-10-25T12:00:00Z</example>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Fecha UTC de la última transición de estado.
    /// </summary>
    /// <example>2026-10-25T12:00:05Z</example>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Fecha UTC de generación cuando el archivo está listo.
    /// </summary>
    /// <example>2026-10-25T12:00:10Z</example>
    public DateTimeOffset? GeneratedAt { get; set; }

    /// <summary>
    /// Fecha UTC de los datos usados por el generador.
    /// </summary>
    /// <example>2026-10-25T12:00:06Z</example>
    public DateTimeOffset? DataAsOf { get; set; }

    /// <summary>
    /// Fin de disponibilidad del archivo temporal; nulo para documentos históricos.
    /// </summary>
    /// <example>2026-10-26T12:00:10Z</example>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Cantidad de intentos de generación iniciados.
    /// </summary>
    /// <example>1</example>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Fecha UTC del próximo intento programado.
    /// </summary>
    /// <example>2026-10-25T12:01:00Z</example>
    public DateTimeOffset? NextAttemptAt { get; set; }

    /// <summary>
    /// Indica que se recuperó un trabajo equivalente existente.
    /// </summary>
    /// <example>true</example>
    public bool Reused { get; set; }

    /// <summary>
    /// Código seguro del último fallo terminal o transitorio.
    /// </summary>
    /// <example>FILE_GENERATION_FAILED</example>
    public string? FailureCode { get; set; }

    /// <summary>
    /// Archivos publicados del trabajo con sus identificadores de descarga.
    /// </summary>
    public IReadOnlyList<FileArtifactSummary> Artifacts { get; set; } = Array.Empty<FileArtifactSummary>();
}
