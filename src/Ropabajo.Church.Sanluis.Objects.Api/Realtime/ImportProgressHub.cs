using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Churc.Sanluis.Framework.Realtime;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Api.Realtime;

/// <summary>
/// Canal de contadores propios de la iglesia validada al preparar el ticket.
/// </summary>
[Authorize(AuthenticationSchemes = ConnectionTicketStore.Scheme)]
public sealed class ImportProgressHub : Hub
{
    private readonly ImportProgressConnections _connections;
    private readonly IMediatorBus _bus;
    private readonly IMapper _mapper;

    internal ImportProgressHub(
        ImportProgressConnections connections,
        IMediatorBus bus,
        IMapper mapper
    )
    {
        _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc />
    public override Task OnConnectedAsync()
    {
        var claim = Context.User?.FindFirst(ImportProgressTicketIssuer.RequesterClaim)?.Value;
        var requester = claim is null
            ? null
            : JsonSerializer.Deserialize<ImportRequester>(claim, ImportProgressTicketIssuer.Json);
        if (
            requester is null
            || !Guid.TryParse(
                Context.GetHttpContext()?.Request.RouteValues["churchCode"]?.ToString(),
                out var church
            )
            || requester.ChurchCode != church
            || _connections.Add(Context.ConnectionId, requester, Context.Abort) is null
        )
        {
            Context.Abort();
            return Task.CompletedTask;
        }
        return base.OnConnectedAsync();
    }

    /// <summary>
    /// Suscribe hasta cincuenta cargas propias, sin recibir autoridad desde el cliente.
    /// </summary>
    public async Task Subscribe(GetImportProgressRequestDto request)
    {
        var session =
            _connections.Get(Context.ConnectionId)
            ?? throw new HubException("El seguimiento no está disponible.");
        var query = _mapper.Map<GetImportProgressQuery>(request);
        query.Requester = session.Requester;
        var result = await _bus.SendAsync(query, Context.ConnectionAborted);
        if (result.IsFailure)
            throw new HubException("Las cargas no están disponibles en esta sesión.");
        session.Subscribe(request.LoadCodes);
        await Clients.Caller.SendAsync(
            "ProgressChanged",
            session.Changes(result.Value.Items),
            Context.ConnectionAborted
        );
    }

    /// <inheritdoc />
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _connections.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
