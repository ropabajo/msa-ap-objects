using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetObjects;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;
using Ropabajo.Church.Sanluis.Objects.Application.ViewModels;
using System.Net.Mime;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers
{
    /// <summary>
    /// Gestión de objetos MinIO
    /// </summary>
    [ApiController]
    [Route("v1/payroll-objects")]
    public class ObjectController : ApiController
    {
        private readonly IMediatorBus _mediator;

        public ObjectController(
            IMediatorBus mediator,
            INotificationHandler<Header> headers,
            INotificationHandler<Notification> notifications,
            IActionContextAccessor actionContextAccessor
        ) : base(headers, notifications, actionContextAccessor)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener una lista de objetos
        /// </summary>
        /// <remarks>
        /// Obtiene listado de objetos
        /// </remarks>
        /// <response code="201">Solicitud exitosa</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="404">No encontrado</response>
        /// <response code="422">Entidad no procesable</response> 
        /// <response code="500">Error interno del servidor</response>      
        [HttpGet(Name = "GetObjectsAsync")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(BadRequestVm), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnprocessableVm), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> GetObjectsAsync([FromQuery] GetObjectsQuery command)
        {
            await _mediator.SendAsync(command);

            return Response();
        }

        /// <summary>
        /// Obtener una url firmada para cargar un archivo a MinIO
        /// </summary>
        /// <remarks>
        /// Obtiene una url firmada y los parámetros para cargar un archivo a un bucket de MinIO según el formato de carga masiva especificado
        /// </remarks>
        /// <response code="201">Solicitud exitosa</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="404">No encontrado</response>
        /// <response code="422">Entidad no procesable</response> 
        /// <response code="500">Error interno del servidor</response>      
        [HttpPost(Name = "CreateObjectAsync")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(BadRequestVm), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnprocessableVm), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> CreateObjectAsync([FromBody] CreateObjectCommand command)
        {
            await _mediator.SendAsync(command);

            return Response();
        }
    }
}
