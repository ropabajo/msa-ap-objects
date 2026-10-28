using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ropabajo.Churc.Sanluis.Framework.Core.Results;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Api.Filters;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Commands.CreateImportProgressTicket;

namespace Ropabajo.Church.Sanluis.Objects.Api.Controllers;

/// <summary>
/// Prepara el canal de avance usando la misma sesión y autorización que la bandeja.
/// </summary>
[ApiController]
[Authorize(Policy = "ActiveChurchRoute")]
[Authorize(Policy = "IamAdmin")]
[ServiceFilter(typeof(FileJobExceptionFilter))]
[Route("v2/churches/{churchCode:guid}/imports/progress-ticket")]
public sealed class ImportProgressController(IMediatorBus bus, IMapper mapper) : ControllerBase
{
    private readonly IMediatorBus _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    /// <summary>
    /// Emite una entrega opaca de un uso; no acepta identidad ni iglesia del cuerpo.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(Ropabajo.Churc.Sanluis.Framework.Primitives.Result<CreateImportProgressTicketResponseDto>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> CreateAsync(
        [FromRoute] Guid churchCode,
        CancellationToken cancellationToken
    )
    {
        Response.Headers.CacheControl = "no-store";
        var command = _mapper.Map<CreateImportProgressTicketCommand>(
            new CreateImportProgressTicketRequestDto { ChurchCode = churchCode }
        );
        return (await _bus.SendAsync(command, cancellationToken)).ToActionResult(
            httpContext: HttpContext
        );
    }
}
