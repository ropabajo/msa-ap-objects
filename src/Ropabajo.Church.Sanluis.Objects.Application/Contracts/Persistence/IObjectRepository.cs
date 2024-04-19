using Object = Ropabajo.Church.Sanluis.Objects.Domain.Entities.Object;

namespace Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence
{
    public interface IObjectRepository : IBaseRepository<Object>
    {
        Task<IEnumerable<Object>> GetByCodeAsync(Guid? code, string? objectName);
    }
}
