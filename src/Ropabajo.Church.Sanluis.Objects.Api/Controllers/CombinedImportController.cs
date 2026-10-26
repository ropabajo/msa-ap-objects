using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ropabajo.Churc.Sanluis.Framework.Core.Results;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateCombinedImport;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImports;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.ListCombinedImportRows;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.RetryCombinedImportRow;
namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers;

/// <summary>
/// Adapta el registro y seguimiento de una única carga para personas y miembros.
/// </summary>
[ApiController]
[Authorize(Policy = "ActiveChurchRoute")]
[Authorize(Policy = "IamAdmin")]
[RequestSizeLimit(65536)]
[ServiceFilter(typeof(FileJobExceptionFilter))]
[Route("v2/churches/{churchCode:guid}/imports")]
public sealed class CombinedImportController : ControllerBase
{
    private readonly IMediatorBus _bus;
    private readonly IMapper _mapper;

    /// <summary>
    /// Recibe el despachador y los mapas de transporte.
    /// </summary>
    public CombinedImportController(IMediatorBus bus, IMapper mapper)
    { _bus = bus ?? throw new ArgumentNullException(nameof(bus)); _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); }

    /// <summary>
    /// Acepta un archivo subido; no declara importadas sus filas.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateAsync([FromRoute] Guid churchCode, [FromBody] CreateCombinedImportRequestDto request, CancellationToken cancellationToken)
    {
        request.ChurchCode = churchCode;
        return (await _bus.SendAsync(_mapper.Map<CreateCombinedImportCommand>(request), cancellationToken)).ToActionResult(StatusCodes.Status202Accepted, httpContext: HttpContext);
    }

    /// <summary>
    /// Lista cargas propias de la iglesia autorizada con conteos finales.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> ListAsync([FromRoute] Guid churchCode, [FromQuery] ListCombinedImportsRequestDto request, CancellationToken cancellationToken)
    {
        request.ChurchCode = churchCode;
        return (await _bus.SendAsync(_mapper.Map<ListCombinedImportsQuery>(request), cancellationToken)).ToActionResult(httpContext: HttpContext);
    }

    /// <summary>
    /// Lista resultados propios sin exponer mensajes internos ni datos de otra carga.
    /// </summary>
    [HttpGet("{loadCode:guid}/rows")]
    public async Task<ActionResult> RowsAsync([FromRoute] Guid churchCode, [FromRoute] Guid loadCode, [FromQuery] ListCombinedImportRowsRequestDto request, CancellationToken cancellationToken)
    {
        request.ChurchCode = churchCode; request.LoadCode = loadCode;
        return (await _bus.SendAsync(_mapper.Map<ListCombinedImportRowsQuery>(request), cancellationToken)).ToActionResult(httpContext: HttpContext);
    }

    /// <summary>
    /// Recupera el último paso observado conservando Persona cuando ya fue confirmada.
    /// </summary>
    [HttpPost("{loadCode:guid}/rows/{rowCode:guid}/retry")]
    public async Task<ActionResult> RetryAsync([FromRoute] Guid churchCode, [FromRoute] Guid loadCode, [FromRoute] Guid rowCode, [FromBody] RetryCombinedImportRowRequestDto request, CancellationToken cancellationToken)
    {
        request.ChurchCode = churchCode; request.LoadCode = loadCode; request.RowCode = rowCode;
        return (await _bus.SendAsync(_mapper.Map<RetryCombinedImportRowCommand>(request), cancellationToken)).ToActionResult(StatusCodes.Status202Accepted, httpContext: HttpContext);
    }
}
