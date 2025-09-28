using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetPagedBulkLoads
{
    public class GetPagedBulkLoadsQuery : Query<IEnumerable<PagedBulkLoadsVm>>
    {
        /// <summary>
        /// Código único de formato de carga masiva
        /// </summary>
        /// <example>82d1d1db-0727-421e-a0fe-78750146f433</example>
        public Guid? FormatCode { get; set; }

        /// <summary>
        /// Número de página
        /// </summary>
        /// <example>1</example>
        public int? PageNumber { get; set; }

        /// <summary>
        /// Tamaño de página
        /// </summary>
        /// <example>10</example>
        public int? PageSize { get; set; }
    }
}
