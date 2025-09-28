using Ropabajo.Churc.Sanluis.Framework.Mediator;

namespace Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadStates.Queries.GetBulkLoadStates
{
    public class GetBulkLoadStatesQuery : Query<IEnumerable<BulkLoadStatesVm>>
    {
        /// <summary>
        /// Código único de la carga masiva
        /// </summary>
        /// <example>25e6dde1-f6a7-4486-96ac-7672641db173</example>
        public Guid BulkLoadCode { get; set; }
    }
}
