using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetPagedBulkLoads
{
    public class GetPagedBulkLoadsQuery : Query<IEnumerable<PagedBulkLoadsVm>>
    {
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
