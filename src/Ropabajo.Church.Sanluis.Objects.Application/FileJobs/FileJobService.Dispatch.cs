using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs;

public sealed partial class FileJobService
{
    /// <summary>
    /// Reclama brevemente una entrada outbox sin mantener la transacción durante el publish.
    /// </summary>
    public Task<FileJobOutbox?> ClaimDispatchAsync(Guid messageId, Guid dispatcherId, CancellationToken cancellationToken)
        => _store.WriteAsync<FileJobOutbox?>(async (session, token) =>
        {
            var message = await session.MessageAsync(messageId, token);
            if (message is null || message.PublishedAt is not null || message.DiscardedAt is not null || message.AvailableAt > Now || message.DispatchUntil > Now) return null;
            var job = await session.JobAsync(message.JobId, token);
            if (job is null || job.Environment != _settings.Environment || !Active(job.Status))
            { message.DiscardedAt = Now; message.Version++; return null; }
            message.DispatchId = dispatcherId; message.DispatchUntil = Now.AddSeconds(60); message.Attempts++; message.Version++;
            return message;
        }, cancellationToken);

    /// <summary>
    /// Registra únicamente la confirmación recibida del broker para el despacho vigente.
    /// </summary>
    public Task<bool> ConfirmDispatchAsync(Guid messageId, Guid dispatcherId, CancellationToken cancellationToken)
        => _store.WriteAsync(async (session, token) =>
        {
            var message = await session.MessageAsync(messageId, token);
            if (message is null || message.DispatchId != dispatcherId || message.PublishedAt is not null) return false;
            message.PublishedAt = Now; message.DispatchUntil = null; message.Version++;
            return true;
        }, cancellationToken);

    /// <summary>
    /// Programa otra publicación cuando no hubo confirmación del broker.
    /// </summary>
    public Task<bool> RetryDispatchAsync(Guid messageId, Guid dispatcherId, CancellationToken cancellationToken)
        => _store.WriteAsync(async (session, token) =>
        {
            var message = await session.MessageAsync(messageId, token);
            if (message is null || message.DispatchId != dispatcherId || message.PublishedAt is not null) return false;
            message.DispatchUntil = null; message.AvailableAt = Now.AddSeconds(Math.Min(300, 5 * Math.Max(1, message.Attempts)) + Random.Shared.Next(5)); message.Version++;
            return true;
        }, cancellationToken);
}
