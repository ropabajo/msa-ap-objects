using Ropabajo.Church.Sanluis.Objects.Domain;

namespace Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence
{
    public interface IDepartmentRepository : IBaseRepository<Departament>
    {
        Task<IEnumerable<Departament>> GetByCodeAsync(int? code, string? description);
    }
}
