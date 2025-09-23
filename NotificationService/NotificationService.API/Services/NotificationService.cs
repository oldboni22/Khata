using Messages.Models;
using NotificationService.Domain.Contracts.Repos;

namespace NotificationService.API.Services;

public interface INotificationService
{
    Task CreateNotificationsAsync(IEnumerable<Notification> notifications);
}

public class NotificationService(IGenericRepository<Notification> repository) : INotificationService
{
    public Task CreateNotificationsAsync(IEnumerable<Notification> notifications)
    {
        throw new NotImplementedException();
    }
}
