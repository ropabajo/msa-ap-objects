using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ropabajo.Churc.Sanluis.Framework.Core.Results;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.Primitives;
using Ropabajo.Church.Sanluis.Objects.Api.Filters;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateCombinedImport;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.RetryCombinedImportRow;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImportRows;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImports;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers;

/// <summary>
/// Registra y consulta las cargas de personas y miembros del solicitante en su iglesia autorizada.
/// </summary>
/// <remarks>
/// Recibe referencias a archivos previamente subidos, no el contenido del Excel.
/// Los identificadores de ruta prevalecen sobre los del cuerpo o la consulta.
/// Las políticas de autorización comprueban la iglesia activa y el permiso administrativo;
/// Application comprueba el propietario y las reglas de cada operación.
/// Las respuestas exitosas conservan el sobre Result. Un HTTP 202 confirma aceptación,
/// no que las filas ya hayan completado los pasos Persona y Miembro.
/// </remarks>
[ApiController]
[Authorize(Policy = "ActiveChurchRoute")]
[Authorize(Policy = "IamAdmin")]
[RequestSizeLimit(MaxRequestBodyBytes)]
[ServiceFilter(typeof(FileJobExceptionFilter))]
[Route("v2/churches/{churchCode:guid}/imports")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
public sealed class CombinedImportController : ControllerBase
{
    private const int MaxRequestBodyBytes = 64 * 1024;

    private readonly IMediatorBus _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Inicializa el transporte HTTP sin incorporar reglas de importación al controlador.
    /// </summary>
    /// <param name="mediator">Despachador de comandos y consultas de Application.</param>
    /// <param name="mapper">Mapeador de los DTO HTTP a los contratos de cada caso de uso.</param>
    /// <exception cref="ArgumentNullException">Una dependencia requerida es nula.</exception>
    public CombinedImportController(IMediatorBus mediator, IMapper mapper)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Acepta una carga sobre un archivo previamente subido por el solicitante.
    /// </summary>
    /// <remarks>
    /// RequestId identifica la intención y debe conservarse al repetir la misma solicitud.
    /// Una intención ya usada con otro archivo o parámetros devuelve conflicto.
    /// El límite de 64 KiB corresponde al JSON de metadatos; el archivo se sube por separado.
    /// </remarks>
    /// <param name="churchCode">Código público de la iglesia tomado de la ruta autorizada.</param>
    /// <param name="request">Intención estable, objeto subido, formato y descripción de la carga.</param>
    /// <param name="cancellationToken">Token de cancelación de la aceptación HTTP.</param>
    /// <returns>Código y estado de la carga aceptada dentro del sobre Result.</returns>
    /// <response code="202">Carga registrada o recuperada por su intención; las filas se procesan de forma asíncrona.</response>
    /// <response code="400">Identificadores, formato o metadatos inválidos.</response>
    /// <response code="401">Se requiere una sesión autenticada válida.</response>
    /// <response code="403">La identidad carece de permiso o no pertenece al ámbito autorizado.</response>
    /// <response code="404">El archivo no está disponible para el solicitante o no terminó de subirse.</response>
    /// <response code="409">La intención o el archivo ya están asociados a otra solicitud incompatible.</response>
    /// <response code="413">El cuerpo JSON supera el límite de metadatos permitido.</response>
    /// <response code="415">El cuerpo utiliza un tipo de contenido no admitido.</response>
    /// <response code="500">Error inesperado; el cuerpo no incluye detalles privados de infraestructura.</response>
    /// <response code="503">El almacenamiento, la persistencia o el contexto de autorización no están disponibles.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Result<CreateCombinedImportResponseDto>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult> CreateAsync(
        [FromRoute] Guid churchCode,
        [FromBody] CreateCombinedImportRequestDto request,
        CancellationToken cancellationToken)
    {
        request.ChurchCode = churchCode;

        var command = _mapper.Map<CreateCombinedImportCommand>(request);
        var result = await _mediator.SendAsync(command, cancellationToken);

        return result.ToActionResult(StatusCodes.Status202Accepted, httpContext: HttpContext);
    }

    /// <summary>
    /// Lista una página de las cargas del solicitante en la iglesia autorizada.
    /// </summary>
    /// <remarks>
    /// El filtro de formato es opcional. Una búsqueda sin coincidencias devuelve una
    /// página vacía con total cero; no representa un recurso inexistente.
    /// El solicitante se obtiene de su sesión, no de parámetros suministrados por el cliente.
    /// </remarks>
    /// <param name="churchCode">Código público de la iglesia tomado de la ruta autorizada.</param>
    /// <param name="request">Filtro de formato y parámetros de paginación.</param>
    /// <param name="cancellationToken">Token de cancelación de la consulta.</param>
    /// <returns>Página y total de cargas autorizadas dentro del sobre Result.</returns>
    /// <response code="200">Página solicitada, incluso cuando no contiene cargas.</response>
    /// <response code="400">Identificador de iglesia, filtro de formato o parámetros de paginación inválidos.</response>
    /// <response code="401">Se requiere una sesión autenticada válida.</response>
    /// <response code="403">La identidad carece de permiso o no pertenece al ámbito autorizado.</response>
    /// <response code="413">La solicitud supera el límite de cuerpo permitido.</response>
    /// <response code="500">Error inesperado; el cuerpo no incluye detalles privados de infraestructura.</response>
    /// <response code="503">La persistencia o el contexto de autorización no están disponibles.</response>
    [HttpGet]
    [ProducesResponseType(typeof(Result<ListCombinedImportsResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> ListAsync(
        [FromRoute] Guid churchCode,
        [FromQuery] ListCombinedImportsRequestDto request,
        CancellationToken cancellationToken)
    {
        request.ChurchCode = churchCode;

        var query = _mapper.Map<ListCombinedImportsQuery>(request);
        var result = await _mediator.SendAsync(query, cancellationToken);

        return result.ToActionResult(httpContext: HttpContext);
    }

    /// <summary>
    /// Consulta una página de resultados de una carga propia sin exponer sus mensajes internos.
    /// </summary>
    /// <remarks>
    /// Los códigos de iglesia y carga se enlazan desde la ruta antes de mapear.
    /// Las filas incluyen su número real en Excel y las observaciones permitidas al solicitante.
    /// Una carga ajena o inexistente produce la misma respuesta de recurso no encontrado.
    /// </remarks>
    /// <param name="churchCode">Código público de la iglesia tomado de la ruta autorizada.</param>
    /// <param name="loadCode">Código público de la carga cuya propiedad se comprueba en Application.</param>
    /// <param name="request">Filtro de resultado y parámetros de paginación de las filas.</param>
    /// <param name="cancellationToken">Token de cancelación de la consulta.</param>
    /// <returns>Página y total de filas autorizadas dentro del sobre Result.</returns>
    /// <response code="200">Resultados de la página solicitada, incluidos los resultados vacíos.</response>
    /// <response code="400">Identificadores, estado de filtro o paginación inválidos.</response>
    /// <response code="401">Se requiere una sesión autenticada válida.</response>
    /// <response code="403">La identidad carece de permiso o no pertenece al ámbito autorizado.</response>
    /// <response code="404">La carga no existe en el ámbito del solicitante.</response>
    /// <response code="413">La solicitud supera el límite de cuerpo permitido.</response>
    /// <response code="500">Error inesperado; el cuerpo no incluye detalles privados de infraestructura.</response>
    /// <response code="503">La persistencia o el contexto de autorización no están disponibles.</response>
    [HttpGet("{loadCode:guid}/rows")]
    [ProducesResponseType(typeof(Result<ListCombinedImportRowsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RowsAsync(
        [FromRoute] Guid churchCode,
        [FromRoute] Guid loadCode,
        [FromQuery] ListCombinedImportRowsRequestDto request,
        CancellationToken cancellationToken)
    {
        request.ChurchCode = churchCode;
        request.LoadCode = loadCode;

        var query = _mapper.Map<ListCombinedImportRowsQuery>(request);
        var result = await _mediator.SendAsync(query, cancellationToken);

        return result.ToActionResult(httpContext: HttpContext);
    }

    /// <summary>
    /// Acepta recuperar el paso observado de una fila autorizada sin repetir Persona si ya fue confirmada.
    /// </summary>
    /// <remarks>
    /// La intención de recuperación es estable ante reintentos HTTP. Application decide
    /// si corresponde recuperar Persona o Miembro y conserva los pasos ya confirmados.
    /// La respuesta aceptada no garantiza que el procesamiento asíncrono haya terminado.
    /// </remarks>
    /// <param name="churchCode">Código público de la iglesia tomado de la ruta autorizada.</param>
    /// <param name="loadCode">Código público de la carga propia que contiene la fila.</param>
    /// <param name="rowCode">Código público de la fila cuyo paso se solicita recuperar.</param>
    /// <param name="request">Identificador estable de la intención de recuperación.</param>
    /// <param name="cancellationToken">Token de cancelación de la aceptación HTTP.</param>
    /// <returns>Aceptación de la recuperación dentro del sobre Result.</returns>
    /// <response code="202">Recuperación registrada o reconocida como ya pendiente.</response>
    /// <response code="400">Identificadores de ruta o intención inválidos.</response>
    /// <response code="401">Se requiere una sesión autenticada válida.</response>
    /// <response code="403">La identidad carece de permiso o no pertenece al ámbito autorizado.</response>
    /// <response code="404">La carga o la fila no existen en el ámbito del solicitante.</response>
    /// <response code="409">El estado de la fila no admite recuperación o la intención es incompatible.</response>
    /// <response code="413">El cuerpo JSON supera el límite de metadatos permitido.</response>
    /// <response code="415">El cuerpo utiliza un tipo de contenido no admitido.</response>
    /// <response code="500">Error inesperado; el cuerpo no incluye detalles privados de infraestructura.</response>
    /// <response code="503">La persistencia o el contexto de autorización no están disponibles.</response>
    [HttpPost("{loadCode:guid}/rows/{rowCode:guid}/retry")]
    [ProducesResponseType(typeof(Result<RetryCombinedImportRowResponseDto>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult> RetryAsync(
        [FromRoute] Guid churchCode,
        [FromRoute] Guid loadCode,
        [FromRoute] Guid rowCode,
        [FromBody] RetryCombinedImportRowRequestDto request,
        CancellationToken cancellationToken)
    {
        request.ChurchCode = churchCode;
        request.LoadCode = loadCode;
        request.RowCode = rowCode;

        var command = _mapper.Map<RetryCombinedImportRowCommand>(request);
        var result = await _mediator.SendAsync(command, cancellationToken);

        return result.ToActionResult(StatusCodes.Status202Accepted, httpContext: HttpContext);
    }
}
