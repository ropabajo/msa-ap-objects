using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ropabajo.Churc.Sanluis.Framework.Files.Messaging;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Hosting;

/// <summary>
/// Despacha la outbox confirmada y recupera leases o archivos vencidos sin depender del navegador.
/// </summary>
public sealed class FileJobsDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly FileJobsSettings _settings;
    private readonly ILogger<FileJobsDispatcher> _logger;
    private readonly IFileCommandPublisher _publisher;
    private readonly TimeProvider _clock;
    private readonly Guid _dispatcherId = Guid.NewGuid();

    /// <summary>
    /// Inicializa el dispatcher sin abrir conexiones en el constructor.
    /// </summary>
    public FileJobsDispatcher(IServiceScopeFactory scopes, IOptions<FileJobsSettings> settings,
        ILogger<FileJobsDispatcher> logger, IFileCommandPublisher publisher, TimeProvider clock)
    {
        _scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
        _settings = (settings ?? throw new ArgumentNullException(nameof(settings))).Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled) return;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopes.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IFileJobStore>();
                var service = scope.ServiceProvider.GetRequiredService<FileJobService>();
                var now = _clock.GetUtcNow().UtcDateTime;
                foreach (var jobId in await store.DueJobsAsync(_settings.Environment, now, _settings.BatchSize, stoppingToken))
                {
                    try { await service.ReconcileAsync(jobId, stoppingToken); }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                    catch (Exception exception) { _logger.LogWarning("File job recovery deferred. JobId={JobId} FailureType={FailureType}", jobId, exception.GetType().Name); }
                }
                await store.PurgeAsync(_settings.Environment, _clock.GetUtcNow().UtcDateTime.AddDays(-30), _settings.BatchSize, stoppingToken);
            foreach (var messageId in await store.PendingMessagesAsync(_settings.Environment, now, _settings.BatchSize, stoppingToken))
                {
                    var message = await service.ClaimDispatchAsync(messageId, _dispatcherId, stoppingToken);
                    if (message is null) continue;
                    try
                    {
                        var command = GenerateFileCommandCodec.Decode(Encoding.UTF8.GetBytes(message.Payload));
                        if (command.Environment != _settings.Environment || command.JobId != message.JobId || command.MessageId != message.MessageId)
                            throw new InvalidDataException("Outbox command scope is inconsistent.");
                        await _publisher.PublishAsync(command, stoppingToken);
                        await service.ConfirmDispatchAsync(messageId, _dispatcherId, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                    catch (Exception exception)
                    {
                        _logger.LogWarning("File command publication unconfirmed. MessageId={MessageId} FailureType={FailureType}", messageId, exception.GetType().Name);
                        await service.RetryDispatchAsync(messageId, _dispatcherId, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception exception) { _logger.LogWarning("File job dispatcher unavailable. FailureType={FailureType}", exception.GetType().Name); }
            await Task.Delay(TimeSpan.FromSeconds(_settings.PollSeconds), stoppingToken);
        }
    }
}
