using AutoMapper;
using FluentValidation;
using MediatR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetDepartments
{
    internal class GetDepartmentsHandler : QueryHandler, IRequestHandler<GetDepartmentsQuery, IEnumerable<DepartmentsVm>>
    {
        private readonly IMediatorBus _bus;
        private readonly IMapper _mapper;
        private readonly IValidator<GetDepartmentsQuery> _validator;
        private readonly IDepartmentRepository _departmentRepository;

        public GetDepartmentsHandler(
            IDepartmentRepository departmentRepository,
            IValidator<GetDepartmentsQuery> validator,
            IMediatorBus bus,
            IMapper mapper
            ) : base(bus)
        {
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<DepartmentsVm>> Handle(GetDepartmentsQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                await RaiseErrrosAsync(validationResult);
                return null;
            }

            var departaments = await _departmentRepository.GetByCodeAsync(query.Code, query.Description);
            if (!departaments.Any())
            {
                await _bus.RaiseAsync(new Notification(NotificationType.NotContent));
                return null;
            }

            return _mapper.Map<IEnumerable<DepartmentsVm>>(departaments);
        }
    }
}
