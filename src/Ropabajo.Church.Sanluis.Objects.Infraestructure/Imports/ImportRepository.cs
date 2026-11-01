using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;
using Ropabajo.Church.Sanluis.Objects.Domain.Imports;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Imports;

internal sealed class ImportRepository(DatabaseContext context) : IImportRepository
{
    public Task<ImportLoad?> GetAsync(Guid loadCode, CancellationToken cancellationToken)
        => context.Set<ImportLoad>().SingleOrDefaultAsync(x => x.LoadCode == loadCode, cancellationToken);
    public void Add(ImportLoad load) => context.Add(load);
    public async Task<(IReadOnlyList<ImportLoadView> Items, int Total)> ListAsync(ImportRequester requester, Guid? formatCode, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = from metadata in context.Set<ImportLoad>().AsNoTracking()
            join load in context.BulkLoads.AsNoTracking() on metadata.LoadCode equals load.Code
            where metadata.ChurchCode == requester.ChurchCode && metadata.OwnerHash == requester.OwnerHash && !load.Delete
                && (!formatCode.HasValue || load.FormatCode == formatCode)
            select new { Load = load, Metadata = metadata };
        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(x => x.Load.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (rows.Select(x => new ImportLoadView(x.Load, x.Metadata)).ToList(), total);
    }
    public async Task<(IReadOnlyList<ImportRow> Items, int Total)> RowsAsync(Guid loadCode, string? state, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Set<ImportRow>().AsNoTracking().Where(x => x.LoadCode == loadCode);
        if (!string.IsNullOrEmpty(state)) query = state switch
        {
            "OBSERVADO" => query.Where(x => x.Status.EndsWith("observed")),
            "COMPLETADO" => query.Where(x => x.Status == "completed"),
            "PROCESADO" => query.Where(x => x.Status.EndsWith("pending")),
            _ => query.Where(x => x.Status == state)
        };
        return (await query.OrderBy(x => x.RowNumber).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken), await query.CountAsync(cancellationToken));
    }
    public Task<ImportRow?> RowAsync(Guid loadCode, Guid rowCode, CancellationToken cancellationToken)
        => context.Set<ImportRow>().SingleOrDefaultAsync(x => x.LoadCode == loadCode && x.RowCode == rowCode, cancellationToken);
}
