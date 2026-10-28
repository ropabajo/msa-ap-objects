namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Lee únicamente contadores del solicitante dentro de su iglesia autorizada.
/// </summary>
public interface IImportProgressReader
{
    /// <summary>
    /// Devuelve las cargas visibles; las desconocidas y ajenas se excluyen por igual.
    /// </summary>
    Task<IReadOnlyList<ImportProgressSnapshot>> ReadAsync(
        ImportRequester requester,
        IReadOnlyCollection<Guid> codes,
        CancellationToken cancellationToken
    );
}
