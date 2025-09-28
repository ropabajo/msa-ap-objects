using FluentValidation;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadResults.Queries.GetTotalBulkLoadResults
{
    public class GetTotalBulkLoadResultsHandler
        : QueryHandler, IRequestHandler<GetTotalBulkLoadResultsQuery, TotalBulkLoadResultsVm>
    {
        private readonly IMediatorBus _bus;
        private readonly IValidator<GetTotalBulkLoadResultsQuery> _validator;
        private readonly IBulkLoadRepository _bulkLoadRepository;
        private readonly IBulkLoadResultRepository _bulkLoadResultRepository;

        public GetTotalBulkLoadResultsHandler(
            IMediatorBus bus,
            IValidator<GetTotalBulkLoadResultsQuery> validator,
            IBulkLoadRepository bulkLoadRepository,
            IBulkLoadResultRepository bulkLoadResultRepository
        ) : base(bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _bulkLoadRepository = bulkLoadRepository ?? throw new ArgumentNullException(nameof(bulkLoadRepository));
            _bulkLoadResultRepository = bulkLoadResultRepository ?? throw new ArgumentNullException(nameof(bulkLoadResultRepository));
        }

        public async Task<TotalBulkLoadResultsVm> Handle(
            GetTotalBulkLoadResultsQuery query, CancellationToken cancellationToken)
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

            var total = await _bulkLoadResultRepository.GetTotalAsync(
                query.BulkLoadCode,
                query.StateCode
            );

            return new TotalBulkLoadResultsVm { Total = total };
        }
    }
}
