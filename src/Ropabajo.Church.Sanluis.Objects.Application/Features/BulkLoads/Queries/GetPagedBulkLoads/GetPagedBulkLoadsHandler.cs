using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetPagedBulkLoads;

public class GetPagedBulkLoadsHandler : QueryHandler, IRequestHandler<GetPagedBulkLoadsQuery, IEnumerable<PagedBulkLoadsVm>>
{
    private readonly IMediatorBus _bus;
    private readonly IMapper _mapper;
    private readonly IBulkLoadRepository _bulkLoadRepository;

    public GetPagedBulkLoadsHandler(
        IMediatorBus bus,
        IMapper mapper,
        IBulkLoadRepository bulkLoadRepository
        ) : base(bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _bulkLoadRepository = bulkLoadRepository ?? throw new ArgumentNullException(nameof(bulkLoadRepository));
    }

    public async Task<IEnumerable<PagedBulkLoadsVm>> Handle(GetPagedBulkLoadsQuery query, CancellationToken cancellationToken)
    {
        var bulkLoads = await _bulkLoadRepository.GetPagedAsync(
            query.FormatCode,
            query.PageNumber.Value,
            query.PageSize.Value
            );

        return _mapper.Map<IEnumerable<PagedBulkLoadsVm>>(bulkLoads);
    }
}
