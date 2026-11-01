using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImportRows;

/// <summary>
/// Ejecuta el caso usando la identidad autenticada y el ámbito de iglesia de la ruta.
/// </summary>
public sealed class ListCombinedImportRowsQueryHandler : IRequestHandler<ListCombinedImportRowsQuery, Result<ListCombinedImportRowsResponseDto>>
{
    private readonly CombinedImportService _service;
    private readonly IImportActor _actor;
    private readonly IImportRepository _repository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Recibe los colaboradores del caso de importación.
    /// </summary>
    public ListCombinedImportRowsQueryHandler(CombinedImportService service, IImportActor actor, IImportRepository repository, IMapper mapper)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service)); _actor = actor ?? throw new ArgumentNullException(nameof(actor));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository)); _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc />
    public async Task<Result<ListCombinedImportRowsResponseDto>> Handle(ListCombinedImportRowsQuery query, CancellationToken cancellationToken)
    {
        var requester = _actor.GetRequester(query.ChurchCode);
        var authorization = await _service.AuthorizeAsync(requester, query.LoadCode, cancellationToken);
        if (authorization.IsFailure) return Result.Failure<ListCombinedImportRowsResponseDto>(authorization.Errors.ToArray());
        var page = await _repository.RowsAsync(query.LoadCode, query.StateCode, query.PageNumber, query.PageSize, cancellationToken);
        return new ListCombinedImportRowsResponseDto { Items = _mapper.Map<List<ListCombinedImportRowsItemResponseDto>>(page.Items), Total = page.Total };
    }
}
