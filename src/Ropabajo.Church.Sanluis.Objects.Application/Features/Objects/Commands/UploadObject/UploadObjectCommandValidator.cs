using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject
{
    public class UploadObjectCommandValidator : AbstractValidator<UploadObjectCommand>
    {
        public UploadObjectCommandValidator()
        {
            RuleFor(p => p.ObjectCode)
                 .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0101")
                 .Must(BeAValidCodeGuid).WithMessage("{PropertyName} does not have a valid value.").WithErrorCode("400.0301"); ;
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
