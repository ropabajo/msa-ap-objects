using System.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Persistence;

/// <summary>
/// Ejecuta la persistencia de trabajos y outbox con transacciones locales y reintentos de concurrencia.
/// </summary>
public sealed class SqlFileJobStore : IFileJobStore
{
    private readonly IDbContextFactory<FileJobsDbContext> _factory;

    /// <summary>
    /// Inicializa la fábrica del contexto independiente de metadatos de archivos.
    /// </summary>
    public SqlFileJobStore(IDbContextFactory<FileJobsDbContext> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <inheritdoc />
    public async Task<T> WriteAsync<T>(Func<IFileJobSession, CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                await using var context = await _factory.CreateDbContextAsync(cancellationToken);
                await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                var result = await operation(new Session(context), cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception exception) when (attempt < 8 && IsConflict(exception))
            {
                await Task.Delay(Math.Min(500, 25 * (1 << attempt)) + Random.Shared.Next(20, 100), cancellationToken);
            }
        }
    }

    /// <inheritdoc />
    public async Task<FileJob?> FindAsync(Guid jobId, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Jobs.AsNoTracking().Include(value => value.Artifacts).SingleOrDefaultAsync(value => value.JobId == jobId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FileJob>> ListAsync(string environment, Guid churchCode, string ownerHash, long? before, int count, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Jobs.AsNoTracking().Include(value => value.Artifacts)
            .Where(value => value.Environment == environment && value.ChurchCode == churchCode && value.OwnerHash == ownerHash && (before == null || value.Sequence < before))
            .OrderByDescending(value => value.Sequence).Take(count).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> DueJobsAsync(string environment, DateTime now, int count, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Jobs.AsNoTracking().Where(value => value.Environment == environment && (
            value.Status == "ready" && value.ExpiresAt <= now
            || (value.Status == "queued" || value.Status == "running" || value.Status == "retry_wait" || value.Status == "cancel_requested") && value.DeadlineAt <= now
            || (value.Status == "running" || value.Status == "cancel_requested") && value.LeaseUntil <= now
            || value.Artifacts.Any(artifact => artifact.DeletedAt == null && artifact.CreatedAt < now.AddHours(-24) && (!artifact.Published || value.Status == "expired" || value.Status == "failed" || value.Status == "canceled"))))
            .OrderBy(value => value.UpdatedAt).Take(count).Select(value => value.JobId).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> PendingMessagesAsync(string environment, DateTime now, int count, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        return await context.Outbox.AsNoTracking().Where(value => value.PublishedAt == null && value.DiscardedAt == null && context.Jobs.Any(job => job.JobId == value.JobId && job.Environment == environment) && value.AvailableAt <= now && (value.DispatchUntil == null || value.DispatchUntil <= now))
            .OrderBy(value => value.AvailableAt).Take(count).Select(value => value.MessageId).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task PurgeAsync(string environment, DateTime before, int count, CancellationToken cancellationToken)
    {
        await using var context = await _factory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var ids = await context.Jobs.Where(job => job.Environment == environment && job.UpdatedAt < before
            && (job.Status == "expired" || job.Status == "failed" || job.Status == "canceled")
            && !job.Artifacts.Any(artifact => artifact.DeletedAt == null))
            .OrderBy(job => job.UpdatedAt).Take(count).Select(job => job.JobId).ToListAsync(cancellationToken);
        if (ids.Count > 0)
        {
            await context.Intents.Where(value => ids.Contains(value.JobId)).ExecuteDeleteAsync(cancellationToken);
            await context.Slots.Where(value => ids.Contains(value.JobId)).ExecuteDeleteAsync(cancellationToken);
            await context.Outbox.Where(value => ids.Contains(value.JobId)).ExecuteDeleteAsync(cancellationToken);
            await context.Audit.Where(value => ids.Contains(value.JobId)).ExecuteDeleteAsync(cancellationToken);
            await context.Jobs.Where(value => ids.Contains(value.JobId)).ExecuteDeleteAsync(cancellationToken);
        }
        await transaction.CommitAsync(cancellationToken);
    }

    private static bool IsConflict(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
            if (current is DbUpdateConcurrencyException or MySqlException { Number: 1062 or 1213 or 1205 }) return true;
        return false;
    }

    private sealed class Session(FileJobsDbContext context) : IFileJobSession
    {
        public Task<FileJob?> JobAsync(Guid jobId, CancellationToken cancellationToken) => context.Jobs.Include(value => value.Artifacts).SingleOrDefaultAsync(value => value.JobId == jobId, cancellationToken);
        public Task<FileJobIntent?> IntentAsync(string scope, Guid requestId, CancellationToken cancellationToken) => context.Intents.SingleOrDefaultAsync(value => value.Scope == scope && value.RequestId == requestId, cancellationToken);
        public Task<FileJobSlot?> SlotAsync(string fingerprint, CancellationToken cancellationToken) => context.Slots.SingleOrDefaultAsync(value => value.Fingerprint == fingerprint, cancellationToken);
        public Task<FileJobOutbox?> MessageAsync(Guid messageId, CancellationToken cancellationToken) => context.Outbox.SingleOrDefaultAsync(value => value.MessageId == messageId, cancellationToken);
        public Task<int> ActiveCountAsync(string environment, Guid churchCode, string ownerHash, CancellationToken cancellationToken) => context.Jobs.CountAsync(value => value.Environment == environment && value.ChurchCode == churchCode && value.OwnerHash == ownerHash
            && (value.Status == "queued" || value.Status == "running" || value.Status == "retry_wait" || value.Status == "cancel_requested"), cancellationToken);
        public void AddJob(FileJob job) => context.Jobs.Add(job);
        public void AddIntent(FileJobIntent intent) => context.Intents.Add(intent);
        public void AddSlot(FileJobSlot slot) => context.Slots.Add(slot);
        public void AddMessage(FileJobOutbox message) => context.Outbox.Add(message);
        public void AddAudit(FileJobAudit audit) => context.Audit.Add(audit);
    }
}
