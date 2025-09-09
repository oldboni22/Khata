using System.Linq.Expressions;
using Domain.Entities;
using Shared.PagedList;

namespace Domain.Contracts.RepositoryContracts;

public interface IGenericWriteRepository<T>
    where T : EntityBase
{
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateAsync(CancellationToken cancellationToken = default);
}
