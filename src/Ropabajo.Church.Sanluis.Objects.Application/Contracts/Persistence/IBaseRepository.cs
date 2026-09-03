using Ropabajo.Church.Sanluis.Objects.Domain.Common;
using System.Linq.Expressions;

namespace Ropabajo.Church.Sanluis.Objects.Application.Contracts.Persistence;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>>? predicate = null,
                                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                    string? includeString = null,
                                    bool disableTracking = true,
                                    CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>>? predicate = null,
                                   Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                   List<Expression<Func<T, object>>>? includes = null,
                                   bool disableTracking = true,
                                   CancellationToken cancellationToken = default);
    Task<T> GetOneAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    T Add(T entity);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    T Update(T entity);
    Task DeleteAsync(T entity, bool physical = false, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
