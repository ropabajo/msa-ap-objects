using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;
using Object = Ropabajo.Church.Sanluis.Objects.Domain.Entities.Object;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories
{
    public class ObjectRepository : RepositoryBase<Object>, IObjectRepository
    {
        public ObjectRepository(DatabaseContext dbContext) : base(dbContext)
        { }

        public async Task<IEnumerable<Object>> GetByCodeAsync(Guid? code, string? objectName)
        {
            return await _dbContext.Objects.AsNoTracking()
                                .Where(x =>
                                    (!code.HasValue || x.Code == code.Value)
                                    && (string.IsNullOrEmpty(objectName) || x.ObjectName.Contains(objectName))
                                    )
                                .OrderBy(x => x.CreatedDate)
                                .ToListAsync();
        }
    }
}
