using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ropabajo.Church.Sanluis.Objects.Application.Features.TemporaryExports;
namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.TemporaryExports;

/// <summary>Recupera trabajos y limpia objetos temporales incluso con el navegador cerrado.</summary>
public sealed class TemporaryExportWorker(IServiceScopeFactory scopes, IOptions<TemporaryExportOptions> settings,
    ILogger<TemporaryExportWorker> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!settings.Value.Enabled) return;
        string? cursor = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                cursor = await scope.ServiceProvider.GetRequiredService<TemporaryExportCoordinator>().ReconcileAsync(cursor, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception exception) { logger.LogWarning("Temporary export reconciliation deferred. FailureType={FailureType}", exception.GetType().Name); }
            await Task.Delay(TimeSpan.FromSeconds(settings.Value.PollSeconds), stoppingToken);
        }
    }
}
