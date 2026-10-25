using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ropabajo.Churc.Sanluis.Framework.Core.Results;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CreateFileJob;
using Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.GetFileJob;
using Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Queries.ListFileJobs;
using Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CancelFileJob;
using Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.GetFileDownload;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers;

/// <summary>
/// Adapta solicitudes y consultas de archivos del usuario autorizado.
/// </summary>
[ApiController]
[RequestSizeLimit(65536)]
[Authorize(Policy = "ActiveChurchRoute")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("v1/church/{churchCode:guid}/file-jobs")]
[ServiceFilter(typeof(FileJobExceptionFilter))]
public sealed class FileJobController : ControllerBase
{
    private readonly IMediatorBus _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Inicializa transporte y mapeo sin incorporar reglas de negocio.
    /// </summary>
    public FileJobController(IMediatorBus mediator, IMapper mapper)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Registra o reutiliza una exportación autorizada de la iglesia.
    /// </summary>
    /// <param name="churchCode">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="request">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    /// <returns>Resultado tipado del caso de uso.</returns>
    /// <response code="202">Trabajo aceptado y guardado junto con su intención de despacho.</response>
    /// <response code="400">Identificadores, contrato o parámetros inválidos.</response>
    /// <response code="401">Se requiere una identidad autenticada válida.</response>
    /// <response code="403">La identidad no tiene acceso al ámbito u operación.</response>
    /// <response code="404">No existe el recurso en el ámbito autorizado.</response>
    /// <response code="409">La intención, el estado o el intento vigente impiden completar la operación.</response>
    /// <response code="413">El cuerpo de la solicitud supera el límite permitido.</response>
    /// <response code="422">El reporte o sus archivos no cumplen las reglas funcionales.</response>
    /// <response code="429">Se alcanzó la cuota temporal de trabajos activos.</response>
    /// <response code="503">El mecanismo está desactivado o una dependencia no está disponible.</response>
    /// <response code="500">Error inesperado; utilizar traceId para diagnóstico.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Result<CreateFileJobResponseDto>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreateFileJobAsync(
        [FromRoute] Guid churchCode,
        [FromBody] CreateFileJobRequestDto request,
        CancellationToken cancellationToken)
    {
        request.ChurchCode = churchCode;
        var command = _mapper.Map<CreateFileJobCommand>(request);
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.ToActionResult(StatusCodes.Status202Accepted, httpContext: HttpContext);
    }

    /// <summary>
    /// Consulta el estado y los archivos publicados de un trabajo propio.
    /// </summary>
    /// <param name="request">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    /// <returns>Resultado tipado del caso de uso.</returns>
    /// <response code="200">Operación completada; consultar el resultado tipado.</response>
    /// <response code="400">Identificadores, contrato o parámetros inválidos.</response>
    /// <response code="401">Se requiere una identidad autenticada válida.</response>
    /// <response code="403">La identidad no tiene acceso al ámbito u operación.</response>
    /// <response code="404">No existe el recurso en el ámbito autorizado.</response>
    /// <response code="409">La intención, el estado o el intento vigente impiden completar la operación.</response>
    /// <response code="413">El cuerpo de la solicitud supera el límite permitido.</response>
    /// <response code="422">El reporte o sus archivos no cumplen las reglas funcionales.</response>
    /// <response code="429">Se alcanzó la cuota temporal de trabajos activos.</response>
    /// <response code="503">El mecanismo está desactivado o una dependencia no está disponible.</response>
    /// <response code="500">Error inesperado; utilizar traceId para diagnóstico.</response>
    [HttpGet("{jobId:guid}")]
    [ProducesResponseType(typeof(Result<GetFileJobResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetFileJobAsync(
        [FromRoute] GetFileJobRequestDto request,
        CancellationToken cancellationToken)
    {
        var query = _mapper.Map<GetFileJobQuery>(request);
        var result = await _mediator.SendAsync(query, cancellationToken);
        return result.ToActionResult(StatusCodes.Status200OK, httpContext: HttpContext);
    }

    /// <summary>
    /// Consulta una página del historial privado de archivos.
    /// </summary>
    /// <param name="churchCode">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="request">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    /// <returns>Resultado tipado del caso de uso.</returns>
    /// <response code="200">Operación completada; consultar el resultado tipado.</response>
    /// <response code="400">Identificadores, contrato o parámetros inválidos.</response>
    /// <response code="401">Se requiere una identidad autenticada válida.</response>
    /// <response code="403">La identidad no tiene acceso al ámbito u operación.</response>
    /// <response code="404">No existe el recurso en el ámbito autorizado.</response>
    /// <response code="409">La intención, el estado o el intento vigente impiden completar la operación.</response>
    /// <response code="413">El cuerpo de la solicitud supera el límite permitido.</response>
    /// <response code="422">El reporte o sus archivos no cumplen las reglas funcionales.</response>
    /// <response code="429">Se alcanzó la cuota temporal de trabajos activos.</response>
    /// <response code="503">El mecanismo está desactivado o una dependencia no está disponible.</response>
    /// <response code="500">Error inesperado; utilizar traceId para diagnóstico.</response>
    [HttpGet]
    [ProducesResponseType(typeof(Result<ListFileJobsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ListFileJobsAsync(
        [FromRoute] Guid churchCode,
        [FromQuery] ListFileJobsRequestDto request,
        CancellationToken cancellationToken)
    {
        request.ChurchCode = churchCode;
        var query = _mapper.Map<ListFileJobsQuery>(request);
        var result = await _mediator.SendAsync(query, cancellationToken);
        return result.ToActionResult(StatusCodes.Status200OK, httpContext: HttpContext);
    }

    /// <summary>
    /// Solicita detener una preparación sin modificar datos de negocio.
    /// </summary>
    /// <param name="request">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    /// <returns>Resultado tipado del caso de uso.</returns>
    /// <response code="200">Operación completada; consultar el resultado tipado.</response>
    /// <response code="400">Identificadores, contrato o parámetros inválidos.</response>
    /// <response code="401">Se requiere una identidad autenticada válida.</response>
    /// <response code="403">La identidad no tiene acceso al ámbito u operación.</response>
    /// <response code="404">No existe el recurso en el ámbito autorizado.</response>
    /// <response code="409">La intención, el estado o el intento vigente impiden completar la operación.</response>
    /// <response code="413">El cuerpo de la solicitud supera el límite permitido.</response>
    /// <response code="422">El reporte o sus archivos no cumplen las reglas funcionales.</response>
    /// <response code="429">Se alcanzó la cuota temporal de trabajos activos.</response>
    /// <response code="503">El mecanismo está desactivado o una dependencia no está disponible.</response>
    /// <response code="500">Error inesperado; utilizar traceId para diagnóstico.</response>
    [HttpPost("{jobId:guid}/cancel")]
    [ProducesResponseType(typeof(Result<CancelFileJobResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CancelFileJobAsync(
        [FromRoute] CancelFileJobRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<CancelFileJobCommand>(request);
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.ToActionResult(StatusCodes.Status200OK, httpContext: HttpContext);
    }

    /// <summary>
    /// Firma la descarga del archivo existente sin ampliar su retención.
    /// </summary>
    /// <param name="request">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación.</param>
    /// <returns>Resultado tipado del caso de uso.</returns>
    /// <response code="200">Operación completada; consultar el resultado tipado.</response>
    /// <response code="400">Identificadores, contrato o parámetros inválidos.</response>
    /// <response code="401">Se requiere una identidad autenticada válida.</response>
    /// <response code="403">La identidad no tiene acceso al ámbito u operación.</response>
    /// <response code="404">No existe el recurso en el ámbito autorizado.</response>
    /// <response code="409">La intención, el estado o el intento vigente impiden completar la operación.</response>
    /// <response code="413">El cuerpo de la solicitud supera el límite permitido.</response>
    /// <response code="422">El reporte o sus archivos no cumplen las reglas funcionales.</response>
    /// <response code="429">Se alcanzó la cuota temporal de trabajos activos.</response>
    /// <response code="503">El mecanismo está desactivado o una dependencia no está disponible.</response>
    /// <response code="500">Error inesperado; utilizar traceId para diagnóstico.</response>
    [HttpPost("{jobId:guid}/artifacts/{artifactId:guid}/download")]
    [ProducesResponseType(typeof(Result<GetFileDownloadResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetFileDownloadAsync(
        [FromRoute] GetFileDownloadRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<GetFileDownloadCommand>(request);
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.ToActionResult(StatusCodes.Status200OK, httpContext: HttpContext);
    }
}
