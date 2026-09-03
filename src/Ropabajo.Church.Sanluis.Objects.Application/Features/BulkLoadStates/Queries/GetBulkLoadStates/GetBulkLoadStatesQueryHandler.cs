using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadStates.Queries.GetBulkLoadStates;

public class GetBulkLoadStatesQueryHandler
    : QueryHandler, IRequestHandler<GetBulkLoadStatesQuery, IEnumerable<BulkLoadStatesVm>>
{
    private readonly IMediatorBus _bus;
    private readonly IMapper _mapper;
    private readonly IBulkLoadRepository _bulkLoadRepository;
    private readonly IBulkLoadStateRepository _bulkLoadStateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetBulkLoadStatesQueryHandler"/> class.
    /// </summary>
    /// <param name="bus">The mediator bus.</param>
    /// <param name="mapper">The mapper.</param>
    /// <param name="bulkLoadRepository">The bulk load repository.</param>
    /// <param name="bulkLoadStateRepository">The bulk load state repository.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public GetBulkLoadStatesQueryHandler(
        IMediatorBus bus,
        IMapper mapper,
        IBulkLoadRepository bulkLoadRepository,
        IBulkLoadStateRepository bulkLoadStateRepository
    ) : base(bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _bulkLoadRepository = bulkLoadRepository ?? throw new ArgumentNullException(nameof(bulkLoadRepository));
        _bulkLoadStateRepository = bulkLoadStateRepository ?? throw new ArgumentNullException(nameof(bulkLoadStateRepository));
    }

    public async Task<IEnumerable<BulkLoadStatesVm>> Handle(
        GetBulkLoadStatesQuery query, CancellationToken cancellationToken)
    {
        var bulkLoad = await _bulkLoadRepository.GetOneAsync(x => x.Code == query.BulkLoadCode && !x.Delete, cancellationToken);
        if (bulkLoad is null)
        {
            await _bus.RaiseAsync(new Notification("404", "No se encontró la carga masiva.", NotificationType.BadRequest), cancellationToken);
            return [];
        }

        var bulkLoadStatus = await _bulkLoadStateRepository.GetAsync(x => x.BulkLoadId == bulkLoad.Id, cancellationToken);

        return _mapper.Map<IEnumerable<BulkLoadStatesVm>>(bulkLoadStatus);
    }
}
