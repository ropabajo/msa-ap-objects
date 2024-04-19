using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories
{
    public class ObjectStateRepository : RepositoryBase<ObjectState>, IObjectStateRepository
    {
        public ObjectStateRepository(DatabaseContext dbContext) : base(dbContext)
        {
        }
    }
}
