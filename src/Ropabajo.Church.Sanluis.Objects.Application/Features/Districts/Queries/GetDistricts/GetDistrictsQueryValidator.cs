using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Districts.Queries.GetDistricts
{
    public class GetDistrictsQueryValidator : AbstractValidator<GetDistrictsQuery>
    {
        public GetDistrictsQueryValidator()
        {
            RuleFor(p => p.Code)
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0101");
        }
    }
}
