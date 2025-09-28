using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Commands.CreateBulkLoad;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetPagedBulkLoads;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetTotalBulkLoads;
using Ropabajo.Church.Sanluis.Objects.Application.ViewModels;
using System.Net.Mime;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers
{
    /// <summary>
    /// Gestión de formatos de carga masiva
    /// </summary>
    [ApiController]
    [Route("v1/sanluis-objects")]
    public class BulkLoadController : ApiController
    {
        private readonly IMediatorBus _mediator;

        public BulkLoadController(
            IMediatorBus mediator,
            INotificationHandler<Header> headers,
            INotificationHandler<Notification> notifications,
            IActionContextAccessor actionContextAccessor
        ) : base(headers, notifications, actionContextAccessor)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crear un formato de carga masiva
        /// </summary>
        /// <remarks>
        /// Crea un formato de carga masiva y da inicio al proceso de importiación
        /// </remarks>
        /// <response code="201">Recurso creado</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="404">No encontrado</response>
        /// <response code="422">Entidad no procesable</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpPost("bulk-loads", Name = "CreateBulkLoadAsync")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(BadRequestVm), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnprocessableVm), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> CreateBulkLoadAsync([FromBody] CreateBulkLoadCommand command, CancellationToken cancellation = default)
        {
            await _mediator.SendAsync(command);

            return Response();
        }

        /// <summary>
        /// Obtener una página de formatos de carga masiva
        /// </summary>
        /// <remarks>
        /// Obtiene una página de formatos de carga masiva
        /// </remarks>
        /// <response code="200">Solicitud exitosa</response>
        /// <response code="204">Sin contenido</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>      
        [HttpGet("bulk-loads", Name = "GetPagedBulkLoadsAsync")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(IEnumerable<PagedBulkLoadsVm>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PagedBulkLoadsVm>>> GetPagedBulkLoadsAsync([FromQuery] GetPagedBulkLoadsQuery query, CancellationToken cancellation = default)
        {
            var periods = await _mediator.SendAsync(query);

            return Response(periods);
        }

        /// <summary>
        /// Contar los formatos de carga masiva
        /// </summary>
        /// <remarks>
        /// Ontiene el número total de formatos de carga masiva
        /// </remarks>
        /// <param name="formatCode" example="82d1d1db-0727-421e-a0fe-78750146f433">Código único de formato de carga masiva</param>
        /// <response code="200">Solicitud exitosa</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>          
        [HttpGet("bulk-loads/total", Name = "GetTotalBulkLoadsAsync")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(TotalBulkLoadsVm), StatusCodes.Status200OK)]
        public async Task<ActionResult<TotalBulkLoadsVm>> GetTotalBulkLoadsAsync(Guid? formatCode, CancellationToken cancellation = default)
        {
            var query = new GetTotalBulkLoadsQuery { FormatCode = formatCode };

            var total = await _mediator.SendAsync(query);

            return Response(total);
        }
    }
}
