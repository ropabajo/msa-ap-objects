using Ropabajo.Church.Sanluis.Objects.Domain;

namespace Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence
{
    public interface IDistrictRepository : IBaseRepository<District>
    {
        Task<IEnumerable<District>> GetByCodeAsync(int? code,
                string? description,
                int? departamentCode,
                int? provinceCode);
    }
}
