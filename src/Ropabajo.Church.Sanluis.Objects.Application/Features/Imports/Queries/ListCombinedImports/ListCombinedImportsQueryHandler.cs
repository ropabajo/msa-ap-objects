using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImports;

/// <summary>
/// Ejecuta el caso usando la identidad autenticada y el ámbito de iglesia de la ruta.
/// </summary>
public sealed class ListCombinedImportsQueryHandler : IRequestHandler<ListCombinedImportsQuery, Result<ListCombinedImportsResponseDto>>
{
    private readonly CombinedImportService _service;
    private readonly IImportActor _actor;
    private readonly IImportRepository _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Recibe los colaboradores del caso de importación.
    /// </summary>
    public ListCombinedImportsQueryHandler(CombinedImportService service, IImportActor actor, IImportRepository repository, IMapper mapper)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service)); _actor = actor ?? throw new ArgumentNullException(nameof(actor));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository)); _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc />
    public async Task<Result<ListCombinedImportsResponseDto>> Handle(ListCombinedImportsQuery query, CancellationToken cancellationToken)
    {
        var requester = _actor.GetRequester(query.ChurchCode);
        var page = await _repository.ListAsync(requester, query.FilterFormatCode, query.PageNumber, query.PageSize, cancellationToken);
        return new ListCombinedImportsResponseDto { Items = _mapper.Map<List<ListCombinedImportsItemResponseDto>>(page.Items), Total = page.Total };
    }
}
