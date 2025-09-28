using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories
{
    public class BulkLoadResultRepository : RepositoryBase<BulkLoadResult>, IBulkLoadResultRepository
    {
        public BulkLoadResultRepository(DatabaseContext dbContext) : base(dbContext)
        { }

        public async Task<IEnumerable<BulkLoadResult>> GetPagedAsync(
                    Guid? bulkLoadCode,
                    string? stateCode,
                    int pageNumber,
                    int pageSize
                    )
        {
            var query = from bl in _dbContext.BulkLoadResults
                        where
                            (!bulkLoadCode.HasValue || bl.BulkLoadCode == bulkLoadCode.Value)
                            && (string.IsNullOrEmpty(stateCode) || bl.StateCode.Equals(stateCode, StringComparison.OrdinalIgnoreCase))
                            && !bl.Delete

                        select bl;

            return await query
                .OrderByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalAsync(
            Guid? bulkLoadCode,
            string? stateCode
            )
        {
            var query = from bl in _dbContext.BulkLoadResults
                        where
                            (!bulkLoadCode.HasValue || bl.BulkLoadCode == bulkLoadCode.Value)
                            && (string.IsNullOrEmpty(stateCode) || bl.StateCode.Equals(stateCode, StringComparison.OrdinalIgnoreCase))
                            && !bl.Delete
                        select bl;

            return await query.CountAsync();
        }

    }
}
