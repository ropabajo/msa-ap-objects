using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadResults.Queries.GetPagedBulkLoadResults;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadResults.Queries.GetTotalBulkLoadResults;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Commands.CreateBulkLoad;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetPagedBulkLoads;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Queries.GetTotalBulkLoads;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoadStates.Queries.GetBulkLoadStates;
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
        /// Obtener una página de carga masiva
        /// </summary>
        /// <remarks>
        /// Obtiene una página de carga masiva
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
            var bulkLoads = await _mediator.SendAsync(query);

            return Response(bulkLoads);
        }

        /// <summary>
        /// Contar los carga masiva
        /// </summary>
        /// <remarks>
        /// Obtiene el número total de cargas masivas
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

        /// <summary>
        /// Obtener una página de resultados de carga masiva
        /// </summary>
        /// <remarks>
        /// Obtiene una página de resultados de carga masiva
        /// </remarks>
        /// <param name="bulkLoadCode" example="25e6dde1-f6a7-4486-96ac-7672641db173">Código único de la carga masiva</param>
        /// <response code="200">Solicitud exitosa</response>
        /// <response code="204">Sin contenido</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>      
        [HttpGet("bulk-loads/{bulkLoadCode}/results", Name = "GetPagedBulkLoadResultsAsync")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(IEnumerable<PagedBulkLoadResultsVm>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PagedBulkLoadResultsVm>>> GetPagedBulkLoadResultsAsync(
            [FromRoute] Guid bulkLoadCode,
            [FromQuery] GetPagedBulkLoadResultsQuery query, CancellationToken cancellation = default)
        {
            query.BulkLoadCode = bulkLoadCode;

            var results = await _mediator.SendAsync(query);

            return Response(results);
        }

        /// <summary>
        /// Contar los resultados de cargas masivas
        /// </summary>
        /// <remarks>
        /// Obtiene el número total de resultados de cargas masivas
        /// </remarks>
        /// <param name="bulkLoadCode" example="25e6dde1-f6a7-4486-96ac-7672641db173">Código único de la carga masiva</param>
        /// <response code="200">Solicitud exitosa</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>          
        [HttpGet("bulk-loads/{bulkLoadCode}/results/total", Name = "GetTotalBulkLoadResultsAsync")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(TotalBulkLoadResultsVm), StatusCodes.Status200OK)]
        public async Task<ActionResult<TotalBulkLoadResultsVm>> GetTotalBulkLoadResultsAsync(
            [FromRoute] Guid bulkLoadCode,
            [FromQuery] GetTotalBulkLoadResultsQuery query, CancellationToken cancellation = default)
        {
            query.BulkLoadCode = bulkLoadCode;

            var total = await _mediator.SendAsync(query);

            return Response(total);
        }


        /// <summary>
        /// Obtener un listado de estados de carga masiva
        /// </summary>
        /// <remarks>
        /// Obtiene una lista de estados de carga masiva
        /// </remarks>
        /// <param name="bulkLoadCode" example="25e6dde1-f6a7-4486-96ac-7672641db173">Código único de la carga masiva</param>
        /// <response code="200">Solicitud exitosa</response>
        /// <response code="204">Sin contenido</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>      
        [HttpGet("bulk-loads/{bulkLoadCode}/states", Name = "GetBulkLoadStatesAsync")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(IEnumerable<BulkLoadStatesVm>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BulkLoadStatesVm>>> GetBulkLoadStatesAsync(
            [FromRoute] Guid bulkLoadCode, CancellationToken cancellation = default)
        {
            var query = new GetBulkLoadStatesQuery { BulkLoadCode = bulkLoadCode };

            var results = await _mediator.SendAsync(query);

            return Response(results);
        }

    }
}
