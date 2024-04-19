using FluentValidation;
using System.Text.RegularExpressions;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentCommandValidator()
        {
            RuleFor(p => p.ObjectName)
                .NotEmpty().WithMessage("{PropertyName} is required.").WithErrorCode("400.0301")
                .MaximumLength(500).WithMessage("{PropertyName} must not exceed 500 characters.").WithErrorCode("400.0302")
                .Must(BeAValidExcelObjectName).WithMessage("{ObjectName} is not in a correct Excel format (.xls, .xlsx)");
        }
        private bool BeAValidExcelObjectName(string objectName)
        {
            string fileName = Path.GetFileName(objectName);
            var regex = new Regex(@"^[^<>:""/|?*]+\.(xls|xlsx)$", RegexOptions.IgnoreCase);
            return regex.IsMatch(fileName);
        }
    }
}
