using Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Models;

namespace Ropabajo.Church.Sanluis.Objects.Application.FileJobs.Contracts;

/// <summary>
/// Expone únicamente las operaciones SQL que participan en una transacción de trabajos.
/// </summary>
public interface IFileJobSession
{
    /// <summary>
    /// Obtiene el trabajo y sus artefactos dentro de la transacción.
    /// </summary>
    Task<FileJob?> JobAsync(Guid jobId, CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene una intención previa del mismo ámbito.
    /// </summary>
    Task<FileJobIntent?> IntentAsync(string scope, Guid requestId, CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene el registro único de reutilización de una búsqueda.
    /// </summary>
    Task<FileJobSlot?> SlotAsync(string fingerprint, CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene un mensaje para reclamar o completar su despacho.
    /// </summary>
    Task<FileJobOutbox?> MessageAsync(Guid messageId, CancellationToken cancellationToken);

    /// <summary>
    /// Cuenta trabajos activos dentro de la misma transacción serializable de creación.
    /// </summary>
    Task<int> ActiveCountAsync(string environment, Guid churchCode, string ownerHash, CancellationToken cancellationToken);

    /// <summary>
    /// Agrega un nuevo trabajo a la unidad de persistencia.
    /// </summary>
    void AddJob(FileJob job);

    /// <summary>
    /// Agrega una intención a la unidad de persistencia.
    /// </summary>
    void AddIntent(FileJobIntent intent);

    /// <summary>
    /// Agrega el registro único de reutilización.
    /// </summary>
    void AddSlot(FileJobSlot slot);

    /// <summary>
    /// Agrega un mensaje que se confirma junto con el trabajo.
    /// </summary>
    void AddMessage(FileJobOutbox message);

    /// <summary>
    /// Agrega una transición de auditoría sin contenido privado.
    /// </summary>
    void AddAudit(FileJobAudit audit);
}
