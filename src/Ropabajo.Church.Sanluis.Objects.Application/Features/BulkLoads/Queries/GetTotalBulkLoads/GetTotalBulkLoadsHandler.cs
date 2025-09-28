using FluentValidation;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetTotalBulkLoads
{
    public class GetTotalBulkLoadsHandler : QueryHandler, IRequestHandler<GetTotalBulkLoadsQuery, TotalBulkLoadsVm>
    {
        private readonly IMediatorBus _bus;
        private readonly IBulkLoadRepository _bulkLoadRepository;
        private readonly IValidator<GetTotalBulkLoadsQuery> _validator;

        public GetTotalBulkLoadsHandler(
            IMediatorBus bus,
            IBulkLoadRepository bulkLoadRepository
,
            IValidator<GetTotalBulkLoadsQuery> validator) : base(bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _bulkLoadRepository = bulkLoadRepository ?? throw new ArgumentNullException(nameof(bulkLoadRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<TotalBulkLoadsVm> Handle(GetTotalBulkLoadsQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return null;
            }

            var total = await _bulkLoadRepository.GetTotalAsync(query.FormatCode);

            return new TotalBulkLoadsVm { Total = total };
        }
    }
}
