namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Provinces.Queries.GetProvinces
{
    public class ProvincesVm
    {
        /// <summary>
        /// Código departamento
        /// </summary>
        /// <example>2</example>
        public int DepartamentCode { get; set; }

        /// <summary>
        /// Código provincia
        /// </summary>
        /// <example>2</example>
        public int Code { get; set; }

        /// <summary>
        /// Descripción provincia
        /// </summary>
        /// <example>2</example>
        public string Description { get; set; } = null!;
    }
}
