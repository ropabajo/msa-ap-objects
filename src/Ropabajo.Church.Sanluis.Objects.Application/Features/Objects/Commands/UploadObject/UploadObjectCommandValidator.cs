using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject
{
    public class UploadObjectCommandValidator : AbstractValidator<UploadObjectCommand>
    {
        public UploadObjectCommandValidator()
        {
            RuleFor(p => p.ObjectCode)
                 .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0101");
        }
    }
}
