using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers;

/// <summary>Gestión JSON de archivos temporales; los bytes se descargan exclusivamente desde MinIO.</summary>
[ApiController]
[Authorize(Policy = "ActiveChurchRoute")]
[Route("v1/church/{churchCode:guid}/exports")]
[ServiceFilter(typeof(TemporaryExportExceptionFilter))]
public sealed class TemporaryExportController(IMediatorBus mediator) : ControllerBase
{
    /// <summary>Reserva o reutiliza una exportación del usuario en esta iglesia.</summary>
    [HttpPost]
    public async Task<ActionResult<TemporaryExportResponse>> Reserve(Guid churchCode, [FromBody] ReserveExportRequest request, CancellationToken token)
        => Accepted(await mediator.SendAsync(new ReserveExportCommand(churchCode, request), token));
    /// <summary>Recibe una fuente durable de datos preparada por el MSA de negocio.</summary>
    [HttpPut("{id:guid}/source")]
    [ServiceFilter(typeof(TemporaryExportRequestLimitFilter))]
    public async Task<ActionResult<TemporaryExportResponse>> Supply(Guid churchCode, Guid id, [FromBody] ExportWorkbookSource source, CancellationToken token)
        => Accepted(await mediator.SendAsync(new SupplyExportCommand(churchCode, id, source), token));
    /// <summary>Consulta preparación o disponibilidad del documento propio.</summary>
    [HttpGet("{id:guid}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<TemporaryExportResponse>> Status(Guid churchCode, Guid id, CancellationToken token)
        => Ok(await mediator.SendAsync(new GetExportQuery(churchCode, id), token));
    /// <summary>Emite una firma nueva para el mismo archivo; no extiende su retención.</summary>
    [HttpPost("{id:guid}/download")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<ExportDownloadResponse>> Download(Guid churchCode, Guid id, CancellationToken token)
        => Ok(await mediator.SendAsync(new GetExportDownloadQuery(churchCode, id), token));
}
