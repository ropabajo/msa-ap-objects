using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ropabajo.Churc.Sanluis.Framework.Authz.Authorization;
using Ropabajo.Churc.Sanluis.Framework.Core.Results;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Api.Filters;
using Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ClaimFileJob;
using Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.CompleteFileJob;
using Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.FailFileJob;
using Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.HeartbeatFileJob;
using Ropabajo.Church.Sanluis.Objects.Application.Features.FileJobs.Commands.ReserveFileUpload;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers;

/// <summary>
/// Adapta solicitudes técnicas de generadores autorizados.
/// </summary>
[ApiController]
[RequestSizeLimit(65536)]
[Authorize(AuthenticationSchemes = "FileJobsWorker", Policy = "FileJobsWorker")]
[SkipAuthorizationContext]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("internal/v1/file-jobs")]
[ServiceFilter(typeof(FileJobExceptionFilter))]
public sealed class FileJobWorkerController : ControllerBase
{
    private readonly IMediatorBus _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Inicializa transporte y mapeo sin incorporar reglas de negocio.
    /// </summary>
    public FileJobWorkerController(IMediatorBus mediator, IMapper mapper)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Reclama un intento exclusivo y obtiene su especificación autorizada.
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
    [HttpPost("{jobId:guid}/claim")]
    [ProducesResponseType(typeof(Result<ClaimFileJobResponseDto>), StatusCodes.Status200OK)]
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
    public async Task<ActionResult> ClaimFileJobAsync(
        [FromRoute] ClaimFileJobRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = _mapper.Map<ClaimFileJobCommand>(request);
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.ToActionResult(StatusCodes.Status200OK, httpContext: HttpContext);
    }

    /// <summary>
    /// Renueva la autoridad del intento dentro de su plazo máximo.
    /// </summary>
    /// <param name="request">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="leaseToken">Entrada enlazada desde el transporte autorizado.</param>
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
    [HttpPost("{jobId:guid}/attempts/{attemptId:guid}/heartbeat")]
    [ProducesResponseType(typeof(Result<HeartbeatFileJobResponseDto>), StatusCodes.Status200OK)]
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
    public async Task<ActionResult> HeartbeatFileJobAsync(
        [FromRoute] HeartbeatFileJobRequestDto request,
        [FromHeader(Name = "X-File-Job-Lease")] string? leaseToken,
        CancellationToken cancellationToken)
    {
        request.LeaseToken = leaseToken ?? string.Empty;
        var command = _mapper.Map<HeartbeatFileJobCommand>(request);
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.ToActionResult(StatusCodes.Status200OK, httpContext: HttpContext);
    }

    /// <summary>
    /// Reserva una subida limitada al artefacto del intento vigente.
    /// </summary>
    /// <param name="jobId">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="attemptId">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="leaseToken">Entrada enlazada desde el transporte autorizado.</param>
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
    [HttpPost("{jobId:guid}/attempts/{attemptId:guid}/uploads")]
    [ProducesResponseType(typeof(Result<ReserveFileUploadResponseDto>), StatusCodes.Status200OK)]
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
    public async Task<ActionResult> ReserveFileUploadAsync(
        [FromRoute] Guid jobId,
        [FromRoute] Guid attemptId,
        [FromHeader(Name = "X-File-Job-Lease")] string? leaseToken,
        [FromBody] ReserveFileUploadRequestDto request,
        CancellationToken cancellationToken)
    {
        request.JobId = jobId;
        request.AttemptId = attemptId;
        request.LeaseToken = leaseToken ?? string.Empty;
        var command = _mapper.Map<ReserveFileUploadCommand>(request);
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.ToActionResult(StatusCodes.Status200OK, httpContext: HttpContext);
    }

    /// <summary>
    /// Verifica integridad y publica el conjunto final del intento vigente.
    /// </summary>
    /// <param name="jobId">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="attemptId">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="leaseToken">Entrada enlazada desde el transporte autorizado.</param>
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
    [HttpPost("{jobId:guid}/attempts/{attemptId:guid}/complete")]
    [ProducesResponseType(typeof(Result<CompleteFileJobResponseDto>), StatusCodes.Status200OK)]
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
    public async Task<ActionResult> CompleteFileJobAsync(
        [FromRoute] Guid jobId,
        [FromRoute] Guid attemptId,
        [FromHeader(Name = "X-File-Job-Lease")] string? leaseToken,
        [FromBody] CompleteFileJobRequestDto request,
        CancellationToken cancellationToken)
    {
        request.JobId = jobId;
        request.AttemptId = attemptId;
        request.LeaseToken = leaseToken ?? string.Empty;
        var command = _mapper.Map<CompleteFileJobCommand>(request);
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.ToActionResult(StatusCodes.Status200OK, httpContext: HttpContext);
    }

    /// <summary>
    /// Registra un fallo durable y programa el reintento cuando corresponde.
    /// </summary>
    /// <param name="jobId">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="attemptId">Entrada enlazada desde el transporte autorizado.</param>
    /// <param name="leaseToken">Entrada enlazada desde el transporte autorizado.</param>
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
    [HttpPost("{jobId:guid}/attempts/{attemptId:guid}/fail")]
    [ProducesResponseType(typeof(Result<FailFileJobResponseDto>), StatusCodes.Status200OK)]
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
    public async Task<ActionResult> FailFileJobAsync(
        [FromRoute] Guid jobId,
        [FromRoute] Guid attemptId,
        [FromHeader(Name = "X-File-Job-Lease")] string? leaseToken,
        [FromBody] FailFileJobRequestDto request,
        CancellationToken cancellationToken)
    {
        request.JobId = jobId;
        request.AttemptId = attemptId;
        request.LeaseToken = leaseToken ?? string.Empty;
        var command = _mapper.Map<FailFileJobCommand>(request);
        var result = await _mediator.SendAsync(command, cancellationToken);
        return result.ToActionResult(StatusCodes.Status200OK, httpContext: HttpContext);
    }
}
