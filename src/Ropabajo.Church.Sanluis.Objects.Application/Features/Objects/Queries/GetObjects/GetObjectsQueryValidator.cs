using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetObjects
{
    public class GetObjectsQueryValidator : AbstractValidator<GetObjectsQuery>
    {
        public GetObjectsQueryValidator()
        {
            //RuleFor(p => p.Code)
            //    .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0101");

            RuleFor(p => p.ObjectName)
                .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.").WithErrorCode("400.0302")
                .When(x => !string.IsNullOrEmpty(x.ObjectName));
        }
    }
}
