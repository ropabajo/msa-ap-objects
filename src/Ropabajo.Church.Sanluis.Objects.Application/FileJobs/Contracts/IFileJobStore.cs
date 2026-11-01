using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;

/// <summary>
/// Puerto de persistencia durable con transacciones locales serializables.
/// </summary>
public interface IFileJobStore
{
    /// <summary>
    /// Ejecuta lecturas y escrituras locales como una unidad; el callback no realiza HTTP, broker o MinIO.
    /// </summary>
    /// <typeparam name="T">Tipo de desenlace del caso de uso.</typeparam>
    /// <param name="operation">Operación transaccional e idempotente ante conflictos de concurrencia.</param>
    /// <param name="cancellationToken">Token de cancelación de la transacción.</param>
    Task<T> WriteAsync<T>(Func<IFileJobSession, CancellationToken, Task<T>> operation, CancellationToken cancellationToken);

    /// <summary>
    /// Consulta un trabajo sin bloquearlo durante operaciones externas.
    /// </summary>
    /// <param name="jobId">Identificador del trabajo.</param>
    /// <param name="cancellationToken">Token de cancelación de la consulta.</param>
    Task<FileJob?> FindAsync(Guid jobId, CancellationToken cancellationToken);

    /// <summary>
    /// Consulta una página de trabajos del ámbito autorizado.
    /// </summary>
    /// <param name="environment">Ambiente lógico del servidor.</param>
    /// <param name="churchCode">Iglesia autorizada.</param>
    /// <param name="ownerHash">Propietario autenticado.</param>
    /// <param name="before">Secuencia exclusiva de continuación.</param>
    /// <param name="count">Cantidad máxima de filas.</param>
    /// <param name="cancellationToken">Token de la consulta.</param>
    Task<IReadOnlyList<FileJob>> ListAsync(string environment, Guid churchCode, string ownerHash, long? before, int count, CancellationToken cancellationToken);

    /// <summary>
    /// Encuentra trabajos que requieren vencimiento, recuperación o limpieza.
    /// </summary>
    /// <param name="environment">Ambiente lógico del servidor.</param>
    /// <param name="now">Hora UTC de revisión.</param>
    /// <param name="count">Máximo de trabajos por lote.</param>
    /// <param name="cancellationToken">Token del reconciliador.</param>
    Task<IReadOnlyList<Guid>> DueJobsAsync(string environment, DateTime now, int count, CancellationToken cancellationToken);

    /// <summary>
    /// Depura metadatos terminales antiguos después de retirar todos sus artefactos.
    /// </summary>
    Task PurgeAsync(string environment, DateTime before, int count, CancellationToken cancellationToken);

    /// <summary>
    /// Busca mensajes pendientes sin marcar su publicación como exitosa.
    /// </summary>
    /// <param name="now">Hora UTC actual.</param>
    /// <param name="count">Máximo de mensajes por lote.</param>
    /// <param name="cancellationToken">Token del dispatcher.</param>
    Task<IReadOnlyList<Guid>> PendingMessagesAsync(string environment, DateTime now, int count, CancellationToken cancellationToken);
}
