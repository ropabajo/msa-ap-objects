using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateCombinedImport;

/// <summary>
/// Entrada del caso CreateCombinedImport, enlazada con la iglesia autorizada de la ruta.
/// </summary>
public sealed class CreateCombinedImportCommand : IRequest<Result<CreateCombinedImportResponseDto>>
{
    /// <summary>
    /// Identificador de la iglesia autorizado por la ruta.
    /// </summary>
    /// <example>64f46864-aad9-47ca-b499-af5e2384e70a</example>
    public Guid ChurchCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Identificador estable de la intención para reintentos HTTP.
    /// </summary>
    /// <example>8431e4c1-4a16-46ab-8932-e02851c073dd</example>
    public Guid RequestId { get; set; } = Guid.Empty;

    /// <summary>
    /// Código del objeto subido por la cuenta autenticada.
    /// </summary>
    /// <example>38d423f7-886b-4eaf-a757-37fe5bde5ac0</example>
    public Guid ObjectCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Formato existente de personas y miembros.
    /// </summary>
    /// <example>82d1d1db-0727-421e-a0fe-78750146f433</example>
    public Guid FormatCode { get; set; } = Guid.Empty;

    /// <summary>
    /// Descripción funcional de esta carga.
    /// </summary>
    /// <example>Carga de personas y miembros</example>
    public string? Description { get; set; } = null;
}
