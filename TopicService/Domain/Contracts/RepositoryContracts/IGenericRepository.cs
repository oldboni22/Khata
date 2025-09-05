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
        bool trackChanges = false, 
        CancellationToken cancellationToken = default);
    
    Task<PagedList<T>> FindByConditionWithFilterAsync(
        Expression<Func<T, bool>> expression,
        (Expression<Func<T, object>> predicate, bool isAscending)[] keySelectors, 
        PaginationParameters paginationParameters,
        bool trackChanges = false, 
        CancellationToken cancellationToken = default);
    
    Task<T?> FindByIdAsync(Guid id, bool trackChanges = true, CancellationToken cancellationToken = default);
    
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
