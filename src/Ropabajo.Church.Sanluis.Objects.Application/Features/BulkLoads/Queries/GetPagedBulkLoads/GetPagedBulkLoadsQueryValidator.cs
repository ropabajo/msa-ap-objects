using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetPagedBulkLoads
{
    public class GetPagedBulkLoadsQueryValidator : AbstractValidator<GetPagedBulkLoadsQuery>
    {
        public GetPagedBulkLoadsQueryValidator()
        {
            RuleFor(p => p.PageNumber)
                .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0101")
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0102");

            RuleFor(p => p.PageSize)
                .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0201")
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0202");
        }
    }
}
