using FluentValidation;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Queries.GetObjectPresignedUrl
{
    public class GetObjectPresignedUrlQueryValidator : AbstractValidator<GetObjectPresignedUrlQuery>
    {
        public GetObjectPresignedUrlQueryValidator()
        {
            // Validar que la propiedad Template no esté vacía
            RuleFor(p => p.Template)
                .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0101")
                .MaximumLength(400).WithMessage("{PropertyName} must not exceed 400 characters.").WithErrorCode("400.0102");

            // Validar que la Expiration no esté vacía y sea mayor que 0
            RuleFor(p => p.Expiration)
                .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0201")
                .GreaterThan(0).WithMessage("{PropertyName} must be greater than zero.").WithErrorCode("400.0202");
        }
    }

}
