using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Shared.Enums;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadResults.Queries.GetPagedBulkLoadResults
{
    public class GetPagedBulkLoadResultsQueryValidator : AbstractValidator<GetPagedBulkLoadResultsQuery>
    {
        public GetPagedBulkLoadResultsQueryValidator()
        {
            RuleFor(p => p.PageNumber)
                .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0101")
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0102");

            RuleFor(p => p.PageSize)
                .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0201")
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0202");

            RuleFor(p => p.BulkLoadCode)
                .Must(code => Guid.TryParse(code.ToString(), out _))
                .WithMessage("{PropertyName} must be a valid GUID.")
                .WithErrorCode("400.0301");

            RuleFor(p => p.StateCode)
             .Must(BeAValidStateCode)
             .When(p => !string.IsNullOrWhiteSpace(p.StateCode)) // ✅ solo valida si hay valor
             .WithMessage("{PropertyName} no tiene un valor válido.")
             .WithErrorCode("400.0302");
        }

        private bool BeAValidStateCode(string stateCode)
        {
            return Enum.TryParse(typeof(BulkLoadResultState), stateCode, out var _);
        }
    }
}
