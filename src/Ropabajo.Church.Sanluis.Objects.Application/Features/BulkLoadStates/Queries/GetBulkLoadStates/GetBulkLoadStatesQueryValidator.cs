using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadStates.Queries.GetBulkLoadStates
{
    public class GetBulkLoadStatesQueryValidator : AbstractValidator<GetBulkLoadStatesQuery>
    {
        public GetBulkLoadStatesQueryValidator()
        {
            RuleFor(p => p.BulkLoadCode)
                .Must(code => Guid.TryParse(code.ToString(), out _))
                .WithMessage("{PropertyName} must be a valid GUID.")
                .WithErrorCode("400.0301");
        }
    }
}
