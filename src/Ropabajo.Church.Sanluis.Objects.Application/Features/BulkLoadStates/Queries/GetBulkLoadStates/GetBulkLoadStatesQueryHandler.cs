using AutoMapper;
using FluentValidation;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadStates.Queries.GetBulkLoadStates
{
    public class GetBulkLoadStatesQueryHandler
        : QueryHandler, IRequestHandler<GetBulkLoadStatesQuery, IEnumerable<BulkLoadStatesVm>>
    {
        private readonly IMediatorBus _bus;
        private readonly IMapper _mapper;
        private readonly IValidator<GetBulkLoadStatesQuery> _validator;
        private readonly IBulkLoadRepository _bulkLoadRepository;
        private readonly IBulkLoadStateRepository _bulkLoadStateRepository;

        public GetBulkLoadStatesQueryHandler(
            IMediatorBus bus,
            IMapper mapper,
            IValidator<GetBulkLoadStatesQuery> validator,
            IBulkLoadRepository bulkLoadRepository,
            IBulkLoadStateRepository bulkLoadStateRepository
        ) : base(bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _bulkLoadRepository = bulkLoadRepository ?? throw new ArgumentNullException(nameof(bulkLoadRepository));
            _bulkLoadStateRepository = bulkLoadStateRepository ?? throw new ArgumentNullException(nameof(bulkLoadStateRepository));
        }

        public async Task<IEnumerable<BulkLoadStatesVm>> Handle(
            GetBulkLoadStatesQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return null;
            }

            var bulkLoad = await _bulkLoadRepository.GetOneAsync(x => x.Code == query.BulkLoadCode && !x.Delete);
            if (bulkLoad is null)
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotFound));
                return null;
            }

            var bulkLoadStatus = await _bulkLoadStateRepository.GetAsync(x => x.BulkLoadId == bulkLoad.Id);
            if (!bulkLoadStatus.Any())
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotContent));
                return null;
            }

            return _mapper.Map<IEnumerable<BulkLoadStatesVm>>(bulkLoadStatus);
        }
    }
}
