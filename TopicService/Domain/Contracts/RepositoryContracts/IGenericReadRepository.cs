using System.Linq.Expressions;
using Domain.Entities;
using Shared.PagedList;

namespace Domain.Contracts.RepositoryContracts;

public interface IGenericReadRepository<T> where T : EntityBase
{
    Task<PagedList<T>> FindByConditionAsync(
        Expression<Func<T, bool>> expression,
        PaginationParameters paginationParameters,
        (Expression<Func<T, object>> predicate, bool isAscending)[]? keySelectors = null, 
        bool trackChanges = false, 
        CancellationToken cancellationToken = default);

    Task<T?> FindByIdAsync(Guid id, bool trackChanges = true, CancellationToken cancellationToken = default);
}
