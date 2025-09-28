using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadResults.Queries.GetPagedBulkLoadResults
{
    public class GetPagedBulkLoadResultsQuery : Query<IEnumerable<PagedBulkLoadResultsVm>>
    {
        /// <summary>
        /// Código único de la carga masiva
        /// </summary>
        /// <example>25e6dde1-f6a7-4486-96ac-7672641db173</example>
        [BindNever]
        public Guid BulkLoadCode { get; set; }

        /// <summary>
        /// Estado actual del resultado de la carga masiva
        /// 
        /// **Elementos:**
        /// 
        /// | Código        |     Descripción  |
        /// |---------------|------------------|
        /// |   Observed    |     OBSERVADO    |
        /// |   Processed   |     PROCESADO    |
        /// |   Completed   |     COMPLETADO   |
        /// </summary>
        public string? StateCode { get; set; }

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
