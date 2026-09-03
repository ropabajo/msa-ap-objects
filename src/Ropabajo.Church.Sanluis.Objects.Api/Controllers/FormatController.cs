using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ropabajo.Churc.Sanluis.Framework.Core.Responses;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Formats.Queries.GetFormats;
using System.Net.Mime;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers;

/// <summary>
/// Gestión de formatos
/// </summary>
[Authorize]
[ApiController]
[Route("v1/sanluis-objects")]
public class FormatController : ApiController
{
    private readonly IMediatorBus _mediator;

    /// <summary>
    /// Constructor del controlador de formatos
    /// </summary>
    /// <param name="mediator">Instancia de IMediatorBus para enviar comandos y consultas</param>
    /// <param name="headers">Manejador de notificaciones de encabezados</param>
    /// <param name="notifications">Manejador de notificaciones de errores</param>
    public FormatController(
        IMediatorBus mediator,
        INotificationHandler<Header> headers,
        INotificationHandler<Notification> notifications
    ) : base(headers, notifications)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Listar los formatoss
    /// </summary>
    /// <remarks>
    /// Obtiene los formatos soportados por el sistema
    /// </remarks>
    /// <param name="cancellationToken">Token de cancelación para la operación asincrónica</param>
    /// <response code="200">Solicitud exitosa</response>
    /// <response code="204">Sin contenido</response>
    /// <response code="401">No autorizado</response>
    /// <response code="500">Error interno del servidor</response>          
    [HttpGet("formats", Name = "GetFormatsAsync")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<FormatsVm>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FormatsVm>>> GetFormatsAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetFormatsQuery();

        var formats = await _mediator.SendAsync(query, cancellationToken);

        return Response(formats);
    }
}
