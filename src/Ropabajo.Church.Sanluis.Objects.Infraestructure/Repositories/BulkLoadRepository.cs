using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories
{
    public class BulkLoadRepository : RepositoryBase<BulkLoad>, IBulkLoadRepository
    {
        public BulkLoadRepository(DatabaseContext dbContext) : base(dbContext)
        {        }

        public async Task<IEnumerable<BulkLoad>> GetPagedAsync(
            int pageNumber,
            int pageSize
            )
        {
            var query = from bl in _dbContext.BulkLoads
                        where !bl.Delete
                        select new BulkLoad()
                        {
                            Id = bl.Id,
                            Code = bl.Code,
                            ObjectCode = bl.ObjectCode,
                            Description = bl.Description,
                            StateCode = bl.StateCode,
                            Date = bl.Date,
                            User = bl.User,
                        };
            return await query
                .OrderByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalAsync()
        {
            var query = from bl in _dbContext.BulkLoads
                        where !bl.Delete
                        select bl;
            return await query.CountAsync();
        }
    }
}
