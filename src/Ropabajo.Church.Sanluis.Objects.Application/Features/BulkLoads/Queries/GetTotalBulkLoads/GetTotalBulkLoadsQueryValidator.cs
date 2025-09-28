using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetTotalBulkLoads
{
    public class GetTotalBulkLoadsQueryValidator : AbstractValidator<GetTotalBulkLoadsQuery>
    {
        public GetTotalBulkLoadsQueryValidator()
        {
            RuleFor(p => p.FormatCode)
                .Must(code => !code.HasValue || Guid.TryParse(code.ToString(), out _))
                .WithMessage("{PropertyName} must be a valid GUID.")
                .WithErrorCode("400.0301");
        }
    }
}
