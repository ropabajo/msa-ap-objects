using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Commands.CreateBulkLoad
{
    public class CreateBulkLoadCommandValidator : AbstractValidator<CreateBulkLoadCommand>
    {
        public CreateBulkLoadCommandValidator()
        {
            RuleFor(p => p.FormatCode)
                .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0101")
                .Must(BeAValidCodeGuid).WithMessage("{PropertyName} does not have a valid value.").WithErrorCode("400.0301");

            RuleFor(p => p.ObjectCode)
                .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0201")
                .Must(BeAValidCodeGuid).WithMessage("{PropertyName} does not have a valid value.").WithErrorCode("400.0301");

            RuleFor(p => p.Description)
                .MaximumLength(400).WithMessage("{Description} must not exceed 400 characters.").WithErrorCode("400.0301");
        }

        private bool BeAValidCodeGuid(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                return Guid.TryParse(code, out Guid parsedGuid); ;
            }
            return true;
        }
    }
}
