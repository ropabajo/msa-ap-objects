using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetObjects;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;
using Ropabajo.Church.Sanluis.Objects.Application.ViewModels;
using System.Net.Mime;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers
{
    /// <summary>
    /// Gestión de objetos MinIO
    /// </summary>
    [ApiController]
    [Route("v1/sanluis-objects")]
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
        public async Task<ActionResult> GetObjectsAsync([FromQuery] GetObjectsQuery query)
        {
            var objects = await _mediator.SendAsync(query);

            return Response(objects);
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

        /// <summary>
        /// Actualizar el estado de un objeto a subido
        /// </summary>
        /// <remarks>
        /// Actualiza el estado un objeto de planilla de prefirmado a subido
        /// </remarks>
        /// <param name="objectCode" example="4352279a-d37b-4f80-8bd8-42e018d7a98a">Código único de objeto que necesita ser actualizado</param>
        /// <response code="200">Solicitud exitosa</response>
        /// <response code="400">Solicitud incorrecta</response>
        /// <response code="401">No autorizado</response>
        /// <response code="404">No encontrado</response>
        /// <response code="422">Entidad no procesable</response> 
        /// <response code="500">Error interno del servidor</response>
        [HttpPatch("{objectCode}/state/uploaded", Name = "UploadObjectAsync")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(BadRequestVm), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(UnprocessableVm), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> UploadObjectAsync([FromRoute] string objectCode)
        {
            var command = new UploadObjectCommand(objectCode);

            await _mediator.SendAsync(command);

            return Response();
        }
    }
}
