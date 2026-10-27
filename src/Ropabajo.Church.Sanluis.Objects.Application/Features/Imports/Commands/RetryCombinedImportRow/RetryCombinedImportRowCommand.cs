using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.RetryCombinedImportRow;

/// <summary>
/// Entrada del caso RetryCombinedImportRow, enlazada con la iglesia autorizada de la ruta.
/// </summary>
public sealed class RetryCombinedImportRowCommand : IRequest<Result<RetryCombinedImportRowResponseDto>>
{
    /// <summary>
    /// Identificador de la iglesia autorizado por la ruta.
    /// </summary>
    /// <example>64f46864-aad9-47ca-b499-af5e2384e70a</example>
    public Guid ChurchCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Código público de la carga propia.
    /// </summary>
    /// <example>8431e4c1-4a16-46ab-8932-e02851c073dd</example>
    public Guid LoadCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Código público de la fila por recuperar.
    /// </summary>
    /// <example>16c927f9-c4b4-40cc-9de2-5c6ce9d2c1c4</example>
    public Guid RowCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Identificador estable de la intención para reintentos HTTP.
    /// </summary>
    /// <example>8431e4c1-4a16-46ab-8932-e02851c073dd</example>
    public Guid RequestId { get; set; } = Guid.Empty;
}
