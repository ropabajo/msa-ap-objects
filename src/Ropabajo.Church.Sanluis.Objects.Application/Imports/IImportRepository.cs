using Ropabajo.Church.Sanluis.Objects.Domain.Imports;

namespace Ropabajo.Church.Sanluis.Objects.Application.Imports;

/// <summary>
/// Consulta y registra importaciones dentro del ámbito del solicitante.
/// </summary>
public interface IImportRepository
{
    /// <summary>
    /// Obtiene una intención por su código público.
    /// </summary>
    Task<ImportLoad?> GetAsync(Guid loadCode, CancellationToken cancellationToken);
    /// <summary>
    /// Prepara el registro autorizado para la transacción actual.
    /// </summary>
    void Add(ImportLoad load);
    /// <summary>
    /// Lista solo cargas propias de la iglesia autorizada.
    /// </summary>
    Task<(IReadOnlyList<ImportLoadView> Items, int Total)> ListAsync(ImportRequester requester, Guid? formatCode, int pageNumber, int pageSize, CancellationToken cancellationToken);
    /// <summary>
    /// Lista filas de una carga ya autorizada.
    /// </summary>
    Task<(IReadOnlyList<ImportRow> Items, int Total)> RowsAsync(Guid loadCode, string? state, int pageNumber, int pageSize, CancellationToken cancellationToken);
    /// <summary>
    /// Obtiene una fila de una carga ya autorizada.
    /// </summary>
    Task<ImportRow?> RowAsync(Guid loadCode, Guid rowCode, CancellationToken cancellationToken);
}
