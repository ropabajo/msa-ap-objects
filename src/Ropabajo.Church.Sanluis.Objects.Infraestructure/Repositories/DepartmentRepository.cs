using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories
{
    public class DepartmentRepository : RepositoryBase<Departament>, IDepartmentRepository
    {
        public DepartmentRepository(DatabaseContext dbContext) : base(dbContext)
        { }

        public async Task<IEnumerable<Departament>> GetByCodeAsync(int? code, string? description)
        {
            return await _dbContext.Departamentos.AsNoTracking()
                                .Where(x =>
                                    (!code.HasValue || x.Code == code.Value)
                                    && (string.IsNullOrEmpty(description) || x.Description.Contains(description))
                                    )
                                .OrderBy(x => x.Description)
                                .ToListAsync();
        }
    }
}
