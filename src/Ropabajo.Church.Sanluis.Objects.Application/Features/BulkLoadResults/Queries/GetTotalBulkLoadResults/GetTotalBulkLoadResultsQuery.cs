using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadResults.Queries.GetTotalBulkLoadResults
{
    public class GetTotalBulkLoadResultsQuery : IRequest<TotalBulkLoadResultsVm>
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
    }
}
