namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetDepartments
{
    public class DepartmentsVm
    {
        /// <summary>
        /// Código departamento
        /// </summary>
        /// <example>2</example>
        public int Code { get; set; }

        /// <summary>
        /// Descripción departamento
        /// </summary>
        /// <example>LIMA</example>
        public string Description { get; set; } = null!;
    }
}
