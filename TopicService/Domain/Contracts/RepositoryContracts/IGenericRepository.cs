using System.Linq.Expressions;
using Domain.Entities;
using Shared.PagedList;

namespace Domain.Contracts.RepositoryContracts;

public interface IGenericRepository<T>
    where T : EntityBase
{
    Task<PagedList<T>> FindByConditionAsync(
        Expression<Func<T, bool>> expression,
        PaginationParameters paginationParameters,
        (Expression<Func<T, object>> predicate, bool isAscending)[]? keySelectors = null, 
        bool trackChanges = false, 
        CancellationToken cancellationToken = default);

    Task<T?> FindByIdAsync(Guid id, bool trackChanges = true, CancellationToken cancellationToken = default);

    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateAsync(CancellationToken cancellationToken = default);
}
