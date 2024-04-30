using AutoMapper;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetTotalBulkLoads
{
    public class GetTotalBulkLoadsHandler : QueryHandler, IRequestHandler<GetTotalBulkLoadsQuery, TotalBulkLoadsVm>
    {
        private readonly IMediatorBus _bus;
        private readonly IMapper _mapper;
        private readonly IBulkLoadRepository _bulkLoadRepository;

        public GetTotalBulkLoadsHandler(
            IMediatorBus bus,
            IMapper mapper,
            IBulkLoadRepository bulkLoadRepository
            ) : base(bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _bulkLoadRepository = bulkLoadRepository ?? throw new ArgumentNullException(nameof(bulkLoadRepository));
        }

        public async Task<TotalBulkLoadsVm> Handle(GetTotalBulkLoadsQuery query, CancellationToken cancellationToken)
        {
            var total = await _bulkLoadRepository.GetTotalAsync();

            return new TotalBulkLoadsVm { Total = total };
        }
    }
}
