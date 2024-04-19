using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.Districts.Queries.GetDistricts
{
    public class GetDistrictsQuery : Query<IEnumerable<DistrictsVm>>
    {
        /// <summary>
        /// Código departamento
        /// </summary>
        /// <example>2</example>
        public int? DepartamentCode { get; set; }

        /// <summary>
        /// Código provincia
        /// </summary>
        /// <example>2</example>
        public int? ProvinceCode { get; set; }

        /// <summary>
        /// Código distrito
        /// </summary>
        /// <example>2</example>
        public int? Code { get; set; }

        /// <summary>
        /// Descripción distrito
        /// </summary>
        /// <example>SAN LUIS</example>
        public string? Description { get; set; }
    }
}
