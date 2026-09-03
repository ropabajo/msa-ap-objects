using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadResults.Queries.GetPagedBulkLoadResults;

public class GetPagedBulkLoadResultsHandler
    : QueryHandler, IRequestHandler<GetPagedBulkLoadResultsQuery, IEnumerable<PagedBulkLoadResultsVm>>
{
    private readonly IMediatorBus _bus;
    private readonly IMapper _mapper;
    private readonly IBulkLoadRepository _bulkLoadRepository;
    private readonly IBulkLoadResultRepository _bulkLoadResultRepository;

    public GetPagedBulkLoadResultsHandler(
        IMediatorBus bus,
        IMapper mapper,
        IBulkLoadRepository bulkLoadRepository,
        IBulkLoadResultRepository bulkLoadResultRepository
    ) : base(bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _bulkLoadRepository = bulkLoadRepository ?? throw new ArgumentNullException(nameof(bulkLoadRepository));
        _bulkLoadResultRepository = bulkLoadResultRepository ?? throw new ArgumentNullException(nameof(bulkLoadResultRepository));
    }

    public async Task<IEnumerable<PagedBulkLoadResultsVm>> Handle(
        GetPagedBulkLoadResultsQuery query, CancellationToken cancellationToken)
    {
        var bulkLoad = await _bulkLoadRepository.GetOneAsync(x => x.Code == query.BulkLoadCode && !x.Delete, cancellationToken);

        if (bulkLoad is null)
        {
            await _bus.RaiseAsync(new Notification("404", "No se encontró el BulkLoad", NotificationType.BadRequest), cancellationToken);
            return [];
        }

        var bulkLoadResults = await _bulkLoadResultRepository.GetPagedAsync(
            query.BulkLoadCode,
            query.StateCode,
            query.PageNumber.Value,
            query.PageSize.Value
        );

        return _mapper.Map<IEnumerable<PagedBulkLoadResultsVm>>(bulkLoadResults);
    }
}
