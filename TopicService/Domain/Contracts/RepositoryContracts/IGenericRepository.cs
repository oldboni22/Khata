using System.Linq.Expressions;
using Domain.Entities;
using Shared.PagedList;
using Shared.Search;

namespace Domain.Contracts.RepositoryContracts;

public interface IGenericRepository<T> : IGenericReadOnlyRepository<T>
    where T : EntityBase
{
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateAsync(CancellationToken cancellationToken = default);
}
