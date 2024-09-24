using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Formats.Queries.GetFormats;
using System.Net.Mime;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers
{
    /// <summary>
    /// Gestión de formatos
    /// </summary>
    [ApiController]
    [Route("v1/sanluis-objects")]
    public class FormatController : ApiController
    {
        private readonly IMediatorBus _mediator;

        public FormatController(
            IMediatorBus mediator,
            INotificationHandler<Header> headers,
            INotificationHandler<Notification> notifications,
            IActionContextAccessor actionContextAccessor
        ) : base(headers, notifications, actionContextAccessor)
        {
            _mediator = mediator;
        }


        /// <summary>
        /// Listar los formatoss
        /// </summary>
        /// <remarks>
        /// Obtiene los formatos soportados por el sistema
        /// </remarks>
        /// <response code="200">Solicitud exitosa</response>
        /// <response code="204">Sin contenido</response>
        /// <response code="401">No autorizado</response>
        /// <response code="500">Error interno del servidor</response>          
        [HttpGet("formats", Name = "GetFormatsAsync")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(IEnumerable<FormatsVm>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FormatsVm>>> GetFormatsAsync()
        {
            var query = new GetFormatsQuery();

            var formats = await _mediator.SendAsync(query);

            return Response(formats);
        }
    }
}
