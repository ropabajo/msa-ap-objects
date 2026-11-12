using AutoMapper;
using Ropabajo.Churc.Sanluis.Framework.Core.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ropabajo.Churc.Sanluis.Framework.Core.Responses;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Departments.Queries.GetObjects;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.CreateObject;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Commands.UploadObject;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Objects.Queries.GetObjectPresignedUrl;
using System.Net.Mime;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers;

/// <summary>
/// Gestión de objetos MinIO
/// </summary>
[ApiController]
[Route("v1/sanluis-objects")]
[Authorize]
public class ObjectController : ApiController
{
    private readonly IMediatorBus _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Constructor del controlador de objetos
    /// </summary>
    /// <param name="mediator">Instancia de IMediatorBus para enviar comandos y consultas</param>
    /// <param name="headers">Manejador de notificaciones de encabezados</param>
    /// <param name="notifications">Manejador de notificaciones de errores</param>
    /// <param name="mapper">Mapas de los contratos HTTP a los casos de uso.</param>
    public ObjectController(
        IMediatorBus mediator,
        INotificationHandler<Header> headers,
        INotificationHandler<Notification> notifications, IMapper mapper
    ) : base(headers, notifications)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Obtener una lista de objetos
    /// </summary>
    /// <remarks>
    /// Obtiene listado de objetos
    /// </remarks>
    /// <param name="request">Parámetros para crear un objeto</param>
    /// <param name="cancellationToken">Código de cancelación para la operación asincrónica</param>
    /// <response code="201">Solicitud exitosa</response>
    /// <response code="400">Solicitud incorrecta</response>
    /// <response code="401">No autorizado</response>
    /// <response code="404">No encontrado</response>
    /// <response code="422">Entidad no procesable</response> 
    /// <response code="500">Error interno del servidor</response>      
    [HttpGet(Name = "GetObjectsAsync")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ObjectsVm>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetObjectsAsync(
        [FromQuery] GetObjectsQuery request, CancellationToken cancellationToken = default)
    {
        var objects = await _mediator.SendAsync(request, cancellationToken);

        return Response(objects);
    }

    /// <summary>
    /// Obtener una url firmada para cargar un archivo a MinIO
    /// </summary>
    /// <remarks>
    /// Obtiene una url firmada y los parámetros para cargar un archivo a un bucket de MinIO según el formato de carga masiva especificado
    /// </remarks>
    /// <param name="request">Parámetros para crear un objeto</param>
    /// <param name="cancellationToken">Código de cancelación para la operación asincrónica</param>
    /// <response code="201">Solicitud exitosa</response>
    /// <response code="400">Solicitud incorrecta</response>
    /// <response code="401">No autorizado</response>
    /// <response code="404">No encontrado</response>
    /// <response code="422">Entidad no procesable</response> 
    /// <response code="503">El almacenamiento no pudo completar la carga o su verificación.</response>
    /// <response code="500">Error interno del servidor</response>      
    [HttpPost(Name = "CreateObjectAsync")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateObjectAsync(
        [FromBody] CreateObjectRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.SendAsync(_mapper.Map<CreateObjectCommand>(request), cancellationToken);
        if (result.IsFailure) return result.ToActionResult(httpContext: HttpContext);
        foreach (var header in result.Value.Headers) HttpContext.Response.Headers.Append(header.Key, header.Value);
        HttpContext.Response.Headers.Append("Access-Control-Expose-Headers", string.Join(",", result.Value.Headers.Keys));
        return StatusCode(StatusCodes.Status201Created);
    }

    /// <summary>
    /// Actualizar el estado de un objeto a subido
    /// </summary>
    /// <remarks>
    /// Actualiza el estado un objeto de planilla de prefirmado a subido
    /// </remarks>
    /// <param name="request">Identificador del archivo enlazado desde la ruta.</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <response code="200">Solicitud exitosa</response>
    /// <response code="400">Solicitud incorrecta</response>
    /// <response code="401">No autorizado</response>
    /// <response code="404">No encontrado</response>
    /// <response code="422">Entidad no procesable</response> 
    /// <response code="503">El almacenamiento no pudo completar la carga o su verificación.</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPatch("{objectCode}/state/uploaded", Name = "UploadObjectAsync")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UploadObjectAsync(
        [FromRoute] UploadObjectRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.SendAsync(_mapper.Map<UploadObjectCommand>(request), cancellationToken);
        return result.IsSuccess ? Ok() : result.ToActionResult(httpContext: HttpContext);
    }

    /// <summary>
    /// Obtener la presigned url
    /// </summary>
    /// <remarks>
    /// Poder realizar la descarga del archivo
    /// </remarks>
    /// <param name="request">Parámetros para obtener la URL prefirmada</param>
    /// <param name="cancellationToken">Código de cancelación para la operación asincrónica</param>
    /// <response code="200">Solicitud exitosa</response>
    /// <response code="400">Solicitud incorrecta</response>
    /// <response code="401">No autorizado</response>
    /// <response code="404">No encontrado</response>
    /// <response code="422">Entidad no procesable</response> 
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("get-presigned-url")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(GetObjectPresignedUrlVm), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPresignedUrl(
        [FromQuery] GetObjectPresignedUrlQuery request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.SendAsync(request, cancellationToken);
        return result is null ? Response() : Ok(result);
    }
}
