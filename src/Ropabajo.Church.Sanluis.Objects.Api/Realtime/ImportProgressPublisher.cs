using Microsoft.AspNetCore.SignalR;
using Ropabajo.Churc.Sanluis.Framework.Mediator;
using Ropabajo.Church.Sanluis.Objects.Application.Features.Imports.Queries.GetImportProgress;

namespace Ropabajo.Church.Sanluis.Objects.Api.Realtime;

internal sealed class ImportProgressPublisher(
    ImportProgressConnections connections,
    IServiceScopeFactory scopes,
    IHubContext<ImportProgressHub> hub,
    ILogger<ImportProgressPublisher> logger
) : BackgroundService
{
    private readonly ImportProgressConnections _connections =
        connections ?? throw new ArgumentNullException(nameof(connections));
    private readonly IServiceScopeFactory _scopes =
        scopes ?? throw new ArgumentNullException(nameof(scopes));
    private readonly IHubContext<ImportProgressHub> _hub =
        hub ?? throw new ArgumentNullException(nameof(hub));
    private readonly ILogger<ImportProgressPublisher> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var session in _connections.Snapshot())
            {
                var codes = session.Codes();
                if (codes.Length == 0)
                    continue;
                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(
                        stoppingToken
                    );
                    timeout.CancelAfter(TimeSpan.FromSeconds(2));
                    await using var scope = _scopes.CreateAsyncScope();
                    var result = await scope
                        .ServiceProvider.GetRequiredService<IMediatorBus>()
                        .SendAsync(
                            new GetImportProgressQuery
                            {
                                Requester = session.Requester,
                                LoadCodes = codes,
                            },
                            timeout.Token
                        );
                    if (result.IsFailure)
                    {
                        session.Abort();
                        continue;
                    }
                    var changes = session.Changes(result.Value.Items);
                    if (changes.Length > 0)
                        await _hub
                            .Clients.Client(session.Id)
                            .SendAsync("ProgressChanged", changes, timeout.Token)
                            .WaitAsync(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception error)
                {
                    session.Abort();
                    _logger.LogWarning(
                        "Import progress temporarily unavailable. FailureType={FailureType}",
                        error.GetType().Name
                    );
                }
            }
        }
    }
}
