using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories
{
    public class BulkLoadRepository : RepositoryBase<BulkLoad>, IBulkLoadRepository
    {
        public BulkLoadRepository(DatabaseContext dbContext) : base(dbContext)
        {
        }
    }
}
