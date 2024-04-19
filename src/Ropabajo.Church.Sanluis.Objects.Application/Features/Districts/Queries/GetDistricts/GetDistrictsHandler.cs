using AutoMapper;
using FluentValidation;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Districts.Queries.GetDistricts
{
    internal class GetDistrictsHandler : QueryHandler, IRequestHandler<GetDistrictsQuery, IEnumerable<DistrictsVm>>
    {
        private readonly IMediatorBus _bus;
        private readonly IMapper _mapper;
        private readonly IValidator<GetDistrictsQuery> _validator;
        private readonly IDistrictRepository _districtRepository;

        public GetDistrictsHandler(
            IDistrictRepository districtRepository,
            IValidator<GetDistrictsQuery> validator,
            IMediatorBus bus,
            IMapper mapper
            ) : base(bus)
        {
            _districtRepository = districtRepository ?? throw new ArgumentNullException(nameof(districtRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<DistrictsVm>> Handle(GetDistrictsQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return null;
            }

            var districts = await _districtRepository.GetByCodeAsync(
                                                        query.Code,
                                                        query.Description,
                                                        query.DepartamentCode,
                                                        query.ProvinceCode
                                                        );
            if (!districts.Any())
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotContent));
                return null;
            }

            return _mapper.Map<IEnumerable<DistrictsVm>>(districts);
        }
    }
}
