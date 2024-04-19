using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain.Entities;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories
{
    public class FormatRepository : RepositoryBase<Format>, IFormatRepository
    {
        public FormatRepository(DatabaseContext dbContext) : base(dbContext)
        {
        }
    }
}
