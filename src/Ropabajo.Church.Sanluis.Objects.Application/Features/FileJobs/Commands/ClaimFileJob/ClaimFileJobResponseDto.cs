using Ropabajo.Churc.Sanluis.Framework.Files.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ClaimFileJob;

/// <summary>
/// Contiene la especificación autorizada para un intento del generador.
/// </summary>
public sealed class ClaimFileJobResponseDto
{
    /// <summary>
    /// Identificador del trabajo reclamado.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }

    /// <summary>
    /// Identificador del intento que posee el lease vigente.
    /// </summary>
    /// <example>fa8e9db0-c458-48ba-81e2-d54d94655983</example>
    public Guid AttemptId { get; set; }

    /// <summary>
    /// Credencial temporal del intento; no se guarda ni imprime en logs.
    /// </summary>
    /// <example>0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef</example>
    public string LeaseToken { get; set; } = string.Empty;

    /// <summary>
    /// Vencimiento UTC del permiso de ejecución.
    /// </summary>
    /// <example>2026-10-25T12:02:00Z</example>
    public DateTimeOffset LeaseUntil { get; set; }

    /// <summary>
    /// Límite UTC de ejecución del intento.
    /// </summary>
    /// <example>2026-10-25T12:15:00Z</example>
    public DateTimeOffset DeadlineAt { get; set; }

    /// <summary>
    /// Código de iglesia validado al aceptar el trabajo.
    /// </summary>
    /// <example>8cbddf7f-a8ab-46a0-a832-373f454357cd</example>
    public Guid ChurchCode { get; set; }

    /// <summary>
    /// Reporte permitido para el consumidor técnico.
    /// </summary>
    /// <example>members.excel</example>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// Versión del contrato de parámetros.
    /// </summary>
    /// <example>1</example>
    public int ContractVersion { get; set; } = 1;

    /// <summary>
    /// Parámetros validados del reporte, obtenidos por HTTP interno y nunca desde RabbitMQ.
    /// </summary>
    public System.Text.Json.JsonElement Parameters { get; set; }
}
