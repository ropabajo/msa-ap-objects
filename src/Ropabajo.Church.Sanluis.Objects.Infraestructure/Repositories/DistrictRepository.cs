using Microsoft.EntityFrameworkCore;
using Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;
using Ropabajo.Church.Sanluis.Objects.Domain;
using Ropabajo.Church.Sanluis.Objects.Infraestructure.Persistence;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Repositories
{
    public class DistrictRepository : RepositoryBase<District>, IDistrictRepository
    {
        public DistrictRepository(DatabaseContext dbContext) : base(dbContext)
        { }

        public async Task<IEnumerable<District>> GetByCodeAsync(
                int? code,
                string? description,
                int? departamentCode,
                int? provinceCode
            )
        {
            return await _dbContext.Districts.AsNoTracking()
                                .Where(x =>
                                       (!departamentCode.HasValue || x.DepartamentCode == departamentCode.Value)
                                    && (!provinceCode.HasValue || x.ProvinceCode == provinceCode.Value)
                                    && (!code.HasValue || x.Code == code.Value)
                                    && (string.IsNullOrEmpty(description) || x.Description.Contains(description))
                                )
                                 .OrderBy(x => x.Description)
                                .ToListAsync();
        }
    }
}
