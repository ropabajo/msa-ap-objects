using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ReserveFileUpload;

/// <summary>
/// Ejecuta el caso de uso ReserveFileUpload dentro del ámbito autorizado.
/// </summary>
public sealed class ReserveFileUploadCommand : IRequest<Result<ReserveFileUploadResponseDto>>
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
    /// Nombre lógico del archivo esperado por el reporte.
    /// </summary>
    /// <example>workbook</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tamaño exacto del archivo final.
    /// </summary>
    /// <example>24576</example>
    public long SizeBytes { get; set; }

    /// <summary>
    /// SHA-256 del archivo final.
    /// </summary>
    /// <example>aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa</example>
    public string Sha256 { get; set; } = string.Empty;
}
