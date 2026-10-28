using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;

/// <summary>Consulta acotada con autoridad obtenida del ticket preparado por el servidor.</summary>
public sealed class GetImportProgressQuery : IRequest<Result<GetImportProgressResponseDto>>
{
    /// <summary>Ámbito interno autenticado; nunca se enlaza desde el cuerpo del navegador.</summary>
    public ImportRequester Requester { get; set; } = null!;

    /// <summary>Códigos de las cargas solicitadas.</summary>
    public Guid[] LoadCodes { get; set; } = [];
}
