using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.BulkLoads.Commands.CreateBulkLoad;
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
        public async Task<ActionResult> CreateBulkLoadAsync([FromBody] CreateBulkLoadCommand command)
        {
            await _mediator.SendAsync(command);

            return Response();
        }
    }
}
