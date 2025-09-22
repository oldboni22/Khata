using NotificationService.Domain.Models;

namespace NotificationService.DAL.Contracts.Repos;

public interface IGenericRepository<T> where T : NotificationBase
{
    Task CreateAsync(T notification);
    
    Task CreateManyAsync(IEnumerable<T> notifications);

    Task<T?> FindById(Guid id);

    Task<List<T>> FindAll(Guid userId);

    Task<bool> Delete(Guid id);

    Task<T?> UpdateAsync(T notification);
}
