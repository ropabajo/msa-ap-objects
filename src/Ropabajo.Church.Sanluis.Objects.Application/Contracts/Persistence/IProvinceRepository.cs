using Ropabajo.Church.Sanluis.Objects.Domain;

namespace Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence
{
    public interface IProvinceRepository : IBaseRepository<Province>
    {
        Task<IEnumerable<Province>> GetByCodeAsync(int? code, string? description, int? departamentCode);
    }
}
