using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Application.Imports;
using Ropabajo.Church.Sanluis.Objects.Domain.Imports;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Imports;

internal sealed class ImportProgressReader(DatabaseContext context) : IImportProgressReader
{
    private readonly DatabaseContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IReadOnlyList<ImportProgressSnapshot>> ReadAsync(
        ImportRequester requester,
        IReadOnlyCollection<Guid> codes,
        CancellationToken cancellationToken
    ) =>
        await (
            from metadata in _context.Set<ImportLoad>().AsNoTracking()
            join load in _context.BulkLoads.AsNoTracking() on metadata.LoadCode equals load.Code
            where
                codes.Contains(metadata.LoadCode)
                && metadata.OwnerHash == requester.OwnerHash
                && metadata.ChurchCode == requester.ChurchCode
                && !load.Delete
            select new ImportProgressSnapshot(
                load.Code,
                metadata.Status,
                load.StateCode,
                load.Records ?? 0,
                load.UploadedRecords ?? 0,
                load.ObservedRecords ?? 0,
                DateTime.SpecifyKind(metadata.UpdatedAt, DateTimeKind.Utc),
                metadata.Version
            )
        ).ToListAsync(cancellationToken);
}
