using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;
using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.FileJobs.Persistence;

/// <summary>
/// Evita construir conexiones SQL cuando la característica todavía no fue activada.
/// </summary>
public sealed class DisabledFileJobStore : IFileJobStore
{
    /// <inheritdoc />
    public Task<T> WriteAsync<T>(Func<IFileJobSession, CancellationToken, Task<T>> operation, CancellationToken cancellationToken) => throw Disabled();
    /// <inheritdoc />
    public Task<FileJob?> FindAsync(Guid jobId, CancellationToken cancellationToken) => throw Disabled();
    /// <inheritdoc />
    public Task<IReadOnlyList<FileJob>> ListAsync(string environment, Guid churchCode, string ownerHash, long? before, int count, CancellationToken cancellationToken) => throw Disabled();
    /// <inheritdoc />
    public Task<IReadOnlyList<Guid>> DueJobsAsync(string environment, DateTime now, int count, CancellationToken cancellationToken) => throw Disabled();
    /// <inheritdoc />
    public Task<IReadOnlyList<Guid>> PendingMessagesAsync(string environment, DateTime now, int count, CancellationToken cancellationToken) => throw Disabled();
    private static InvalidOperationException Disabled() => new("File jobs are disabled; no database operation is permitted.");
    /// <inheritdoc />
    public Task PurgeAsync(string environment, DateTime before, int count, CancellationToken cancellationToken) => Task.CompletedTask;
}
