using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories
{
    public class ProvinceRepository : RepositoryBase<Province>, IProvinceRepository
    {
        public ProvinceRepository(DatabaseContext dbContext) : base(dbContext)
        { }

        public async Task<IEnumerable<Province>> GetByCodeAsync(
            int? code,
            string? description,
            int? departamentCode
            )
        {
            return await _dbContext.Provinces.AsNoTracking()
                    .Where(x =>
                        (!departamentCode.HasValue || x.DepartamentCode == departamentCode.Value)
                        && (!code.HasValue || x.Code == code.Value)
                        && (string.IsNullOrEmpty(description) || x.Description.Contains(description))
                    )
                    .OrderBy(x => x.Description)
                    .ToListAsync();
        }
    }
}
