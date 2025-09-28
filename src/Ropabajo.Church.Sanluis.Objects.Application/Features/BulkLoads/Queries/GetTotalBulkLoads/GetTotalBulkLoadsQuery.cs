using MediatR;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetTotalBulkLoads
{
    public class GetTotalBulkLoadsQuery : IRequest<TotalBulkLoadsVm>
    {
        /// <summary>
        /// Código único de formato de carga masiva
        /// </summary>
        /// <example>82d1d1db-0727-421e-a0fe-78750146f433</example>
        public Guid? FormatCode { get; set; }
    }
}
