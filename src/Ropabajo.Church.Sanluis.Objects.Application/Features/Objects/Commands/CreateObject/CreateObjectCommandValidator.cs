using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject
{
    public class CreateObjectCommandValidator : AbstractValidator<CreateObjectCommand>
    {
        public CreateObjectCommandValidator()
        {
            RuleFor(p => p.Path)
                 .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0101")
                 .MaximumLength(400).WithMessage("{PropertyName} must not exceed 400 characters.").WithErrorCode("400.0102");

            RuleFor(p => p.AllowedExtensions)
                 .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0201")
                 .MaximumLength(40).WithMessage("{PropertyName} must not exceed 40 characters.").WithErrorCode("400.0202");

            RuleFor(p => p.MaxLength)
                 .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0301")
                 .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0302");

            RuleFor(p => p.Expiration)
                 .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0401")
                 .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0401");

            RuleFor(p => p.FileName)
                .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0501")
                .MaximumLength(80).WithMessage("{PropertyName} must not exceed 80 characters.").WithErrorCode("400.0502");
        }
    }
}
