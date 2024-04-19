using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentCommand : Command
    {
        /// <summary>
        /// Path del archivo Excel 
        /// </summary>
        /// <example>church/sanluis/transversal/ubigeo.xlsx</example>
        public string? ObjectName { get; set; }
    }
}
