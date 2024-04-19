using AutoMapper;
using FluentValidation;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Provinces.Queries.GetProvinces
{
    internal class GetProvincesHandler : QueryHandler, IRequestHandler<GetProvincesQuery, IEnumerable<ProvincesVm>>
    {
        private readonly IMediatorBus _bus;
        private readonly IMapper _mapper;
        private readonly IValidator<GetProvincesQuery> _validator;
        private readonly IProvinceRepository _provinceRepository;

        public GetProvincesHandler(
            IProvinceRepository provinceRepository,
            IValidator<GetProvincesQuery> validator,
            IMediatorBus bus,
            IMapper mapper
            ) : base(bus)
        {
            _provinceRepository = provinceRepository ?? throw new ArgumentNullException(nameof(provinceRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ProvincesVm>> Handle(GetProvincesQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return null;
            }

            var provinces = await _provinceRepository.GetByCodeAsync(
                                                            query.Code,
                                                            query.Description,
                                                            query.DepartamentCode
                                                            );
            if (!provinces.Any())
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotContent));
                return null;
            }

            return _mapper.Map<IEnumerable<ProvincesVm>>(provinces);
        }
    }
}
