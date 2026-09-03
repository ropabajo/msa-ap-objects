using Ropabajo.Church.Sanluis.Objects.Domain.Entities;

namespace Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence
{
    public interface IBulkLoadResultRepository : IBaseRepository<BulkLoadResult>
    {
        Task<IEnumerable<BulkLoadResult>> GetPagedAsync(
                   Guid? bulkLoadCode,
                   string? stateCode,
                   int pageNumber,
                   int pageSize,
                    CancellationToken cancellationToken = default
                   );

        Task<int> GetTotalAsync(
            Guid? bulkLoadCode,
            string? stateCode,
            CancellationToken cancellationToken = default
            );
    }
}
