using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Provinces.Queries.GetProvinces
{
    public class GetProvincesQueryValidator : AbstractValidator<GetProvincesQuery>
    {
        public GetProvincesQueryValidator()
        {
            RuleFor(p => p.DepartamentCode)
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0101");

            RuleFor(p => p.Code)
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0101");

            RuleFor(p => p.Description)
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.").WithErrorCode("400.0302")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
