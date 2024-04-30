using AutoMapper;
using FluentValidation;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetPagedBulkLoads
{
    public class GetPagedBulkLoadsHandler : QueryHandler, IRequestHandler<GetPagedBulkLoadsQuery, IEnumerable<PagedBulkLoadsVm>>
    {
        private readonly IMediatorBus _bus;
        private readonly IMapper _mapper;
        private readonly IValidator<GetPagedBulkLoadsQuery> _validator;
        private readonly IBulkLoadRepository _bulkLoadRepository;

        public GetPagedBulkLoadsHandler(
            IMediatorBus bus,
            IMapper mapper,
            IValidator<GetPagedBulkLoadsQuery> validator,
            IBulkLoadRepository bulkLoadRepository
            ) : base(bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _bulkLoadRepository = bulkLoadRepository ?? throw new ArgumentNullException(nameof(bulkLoadRepository));
        }

        public async Task<IEnumerable<PagedBulkLoadsVm>> Handle(GetPagedBulkLoadsQuery query, CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return null;
            }

            var bulkLoads = await _bulkLoadRepository.GetPagedAsync(
                query.PageNumber.Value,
                query.PageSize.Value
                );
            if (!bulkLoads.Any())
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotContent));
                return null;
            }

            return _mapper.Map<IEnumerable<PagedBulkLoadsVm>>(bulkLoads);
        }
    }
}
