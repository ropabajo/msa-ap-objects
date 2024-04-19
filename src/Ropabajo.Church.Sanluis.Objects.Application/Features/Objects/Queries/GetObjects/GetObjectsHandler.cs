using AutoMapper;
using FluentValidation;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetObjects
{
    internal class GetObjectsHandler : QueryHandler, IRequestHandler<GetObjectsQuery, IEnumerable<ObjectsVm>>
    {
        private readonly IMediatorBus _bus;
        private readonly IMapper _mapper;
        private readonly IValidator<GetObjectsQuery> _validator;
        private readonly IObjectRepository _objectRepository;

        public GetObjectsHandler(
            IObjectRepository objectRepository,
            IValidator<GetObjectsQuery> validator,
            IMediatorBus bus,
            IMapper mapper
            ) : base(bus)
        {
            _objectRepository = objectRepository ?? throw new ArgumentNullException(nameof(objectRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ObjectsVm>> Handle(GetObjectsQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return null;
            }

            var departaments = await _objectRepository.GetByCodeAsync(query.Code, query.ObjectName);
            if (!departaments.Any())
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotContent));
                return null;
            }

            return _mapper.Map<IEnumerable<ObjectsVm>>(departaments);
        }
    }
}
