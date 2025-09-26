using Messages.Models;

namespace NotificationService.Domain.Contracts.Repos;

public interface IGenericRepository<T> where T : Notification
{
    Task CreateManyAsync(IEnumerable<T> notifications);

    Task<T?> FindById(Guid id);

    Task<List<T>> FindAll(Guid userId);

    Task<bool> Delete(Guid id);

    Task UpdateAsync(T notification);
    
    Task UpdateManyAsync(IEnumerable<T> notifications);
}
