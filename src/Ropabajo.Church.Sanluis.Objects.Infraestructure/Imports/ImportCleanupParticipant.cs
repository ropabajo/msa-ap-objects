using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Domain.Imports;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;
using Ropabajo.Churc.Sanluis.Framework.Integration;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Imports;

/// <summary>Retira contratos y contenido Excel de filas completadas; conserva resumen, códigos y permisos.</summary>
internal sealed class ImportCleanupParticipant(DatabaseContext context) : IIntegrationCleanupParticipant
{
    public async Task<bool> CleanupAsync(Guid correlationId, CancellationToken cancellationToken)
    {
        var row = await context.Set<ImportRow>().SingleOrDefaultAsync(x => x.RowCode == correlationId, cancellationToken);
        if (row is not null)
        {
            if (row.Status != "completed") return false;
            row.PersonRequestJson = "{}"; row.MemberRequestJson = null;
            var result = await context.BulkLoadResults.SingleOrDefaultAsync(x => x.Code == correlationId, cancellationToken);
            if (result is not null) result.Row = "{}";
            return true;
        }
        var load = await context.Set<ImportLoad>().SingleOrDefaultAsync(x => x.LoadCode == correlationId, cancellationToken);
        return load?.Status == "completed";
    }
}
