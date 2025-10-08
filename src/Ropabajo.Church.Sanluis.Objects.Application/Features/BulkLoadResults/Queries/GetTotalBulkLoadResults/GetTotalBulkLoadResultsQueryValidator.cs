using FluentValidation;
using Ropabajo.Church.Sanluis.Objects.Application.Shared.Enums;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadResults.Queries.GetTotalBulkLoadResults
{
    public class GetTotalBulkLoadResultsQueryValidator : AbstractValidator<GetTotalBulkLoadResultsQuery>
    {
        public GetTotalBulkLoadResultsQueryValidator()
        {
            RuleFor(p => p.BulkLoadCode)
                .Must(code => Guid.TryParse(code.ToString(), out _))
                .WithMessage("{PropertyName} must be a valid GUID.")
                .WithErrorCode("400.0301");

            RuleFor(p => p.StateCode)
             .Must(BeAValidStateCode)
             .When(p => !string.IsNullOrWhiteSpace(p.StateCode))
             .WithMessage("{PropertyName} no tiene un valor válido.")
             .WithErrorCode("400.0302");
        }

        private bool BeAValidStateCode(string stateCode)
        {
            return !string.IsNullOrWhiteSpace(stateCode)
                    && BulkLoadResultState.ValidCodes.Contains(stateCode.Trim());
        }
    }
}
