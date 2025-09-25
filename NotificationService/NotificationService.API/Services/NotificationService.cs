using Messages.Models;
using NotificationService.Domain.Contracts.Repos;
using Shared.PagedList;

namespace NotificationService.API.Services;

public interface INotificationService
{
    Task CreateNotificationsAsync(IEnumerable<Notification> notifications);
    
    Task<PagedList<Notification>> FindAllNotificationsAsync(
        Guid userId, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);
    
    Task<PagedList<Notification>> FindUnreadNotificationsAsync(
        Guid userId, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);

    Task MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    
    Task MarkUnreadNotificationsAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class NotificationService(INotificationRepository repository) : INotificationService
{
    public async Task CreateNotificationsAsync(IEnumerable<Notification> notifications)
    {
        await repository.CreateManyAsync(notifications);
    }

    public async Task<PagedList<Notification>> FindAllNotificationsAsync(Guid userId, PaginationParameters? paginationParameters,
        CancellationToken cancellationToken = default)
    {
        paginationParameters ??= new();
        
        return await repository.FindAllNotificationsAsync(userId, paginationParameters, cancellationToken);
    }

    public async Task<PagedList<Notification>> FindUnreadNotificationsAsync(Guid userId, PaginationParameters? paginationParameters,
        CancellationToken cancellationToken = default)
    {
        paginationParameters ??= new();
        
        return await repository.FindUnreadNotificationsAsync(userId, paginationParameters, cancellationToken);
    }

    public async Task MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await repository.FindById(notificationId) 
                           ?? throw new NotImplementedException();

        if (notification.ReadAt is not null)
        {
            throw new NotImplementedException();
        }
        
        notification.CreatedAt = DateTime.UtcNow;
        
        await repository.UpdateAsync(notification);
    }

    public async Task MarkUnreadNotificationsAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await repository.FindUnreadNotificationsAsync(userId, cancellationToken);
        
        var updatedAt = DateTime.UtcNow;
        
        var updatedNotifications = unreadNotifications.Select(notification =>
        {
            notification.ReadAt = updatedAt;
            return notification;
        });
        
        await repository.UpdateManyAsync(updatedNotifications);
    }
}
