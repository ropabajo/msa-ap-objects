using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.FailFileJob;

/// <summary>
/// Ejecuta el caso de uso FailFileJob dentro del ámbito autorizado.
/// </summary>
public sealed class FailFileJobCommand : IRequest<Result<FailFileJobResponseDto>>
{
    /// <summary>
    /// Identificador del trabajo desde la ruta.
    /// </summary>
    /// <example>753e6348-578b-40c5-90ed-86592c1d8b16</example>
    public Guid JobId { get; set; }

    /// <summary>
    /// Identificador del intento desde la ruta.
    /// </summary>
    /// <example>fa8e9db0-c458-48ba-81e2-d54d94655983</example>
    public Guid AttemptId { get; set; }

    /// <summary>
    /// Credencial del intento enlazada desde el encabezado técnico.
    /// </summary>
    /// <example>0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef</example>
    public string LeaseToken { get; set; } = string.Empty;

    /// <summary>
    /// Código seguro y estable del fallo del generador.
    /// </summary>
    /// <example>FILE_GENERATION_FAILED</example>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el fallo es transitorio, sujeto al límite del servidor.
    /// </summary>
    /// <example>true</example>
    public bool Retryable { get; set; }
}
