using Messages.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NotificationService.Domain.Contracts;
using NotificationService.Domain.Contracts.Repos;
using NotificationService.Infrastructure.GRpc;
using NotificationService.Infrastructure.MemoryCache;
using NotificationService.Infrastructure.Socket;
using Shared.Exceptions;
using Shared.PagedList;

namespace NotificationService.API.Services;

public interface INotificationService
{
    Task CreateNotificationsAsync(IEnumerable<Notification> notifications);
    
    Task<PagedList<Notification>> FindAllNotificationsAsync(
        string identityProviderId, PaginationParameters? paginationParameters, CancellationToken cancellationToken = default);
    
    Task<PagedList<Notification>> FindUnreadNotificationsAsync(
        string identityProviderId, PaginationParameters? paginationParameters, CancellationToken cancellationToken = default);

    Task MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    
    Task MarkUnreadNotificationsAsReadAsync(string senderId, CancellationToken cancellationToken = default);
}

public class NotificationService(
    INotificationRepository repository, 
    TimeProvider timeProvider, 
    IUserGrpcService userGrpcService, 
    IHubContext<NotificationHub> hubContext,
    IMemoryCacheService<Guid, string> userIdMemoryCache,
    IOptions<NotificationServiceSocketOptions> socketOptions) : INotificationService
{
    private readonly string _increaseNotificationsIndicatorMethodName = socketOptions.Value.IncreaseNotificationsIndicatorMethodName;
    
    public async Task CreateNotificationsAsync(IEnumerable<Notification> notifications)
    {
        var utcNow = timeProvider.GetUtcNow().DateTime;
        
        var createdNotificationsArray = notifications.Select(notif =>
        {
            notif.CreatedAt = utcNow;
            return notif;
        }).ToArray();
        
        var userIds = createdNotificationsArray
            .Select(notif => userIdMemoryCache.GetValue(notif.UserId) ?? string.Empty)
            .Where(id => !string.IsNullOrEmpty(id));
        
        await repository.CreateManyAsync(createdNotificationsArray);

        await hubContext.Clients.Users(userIds).SendAsync(_increaseNotificationsIndicatorMethodName);
    }

    public async Task<PagedList<Notification>> FindAllNotificationsAsync(
        string identityProviderId, PaginationParameters? paginationParameters, CancellationToken cancellationToken = default)
    {
        paginationParameters ??= new();
        
        var userId = await userGrpcService.GetUserIdAsync(identityProviderId)??
                     throw new NotFoundException();
        
        return await repository.FindAllNotificationsAsync(userId, paginationParameters, cancellationToken);
    }

    public async Task<PagedList<Notification>> FindUnreadNotificationsAsync(
        string identityProviderId, PaginationParameters? paginationParameters, CancellationToken cancellationToken = default)
    {
        paginationParameters ??= new();
        
        var userId = await userGrpcService.GetUserIdAsync(identityProviderId)
                     ?? throw new NotFoundException();
        
        return await repository.FindUnreadNotificationsAsync(userId, paginationParameters, cancellationToken);
    }

    public async Task MarkNotificationAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await repository.FindById(notificationId) 
                           ?? throw new NotFoundException();

        if (notification.ReadAt is not null)
        {
            throw new BadRequestException();
        }
        
        notification.CreatedAt = timeProvider.GetUtcNow().DateTime;
        
        await repository.UpdateAsync(notification);
    }

    public async Task MarkUnreadNotificationsAsReadAsync(string senderId, CancellationToken cancellationToken = default)
    {
        var userId = await userGrpcService.GetUserIdAsync(senderId)??
                     throw new NotFoundException(); 
        
        var unreadNotifications = await repository.FindUnreadNotificationsAsync(userId, cancellationToken);
        
        var updatedAt = timeProvider.GetUtcNow().DateTime;
        
        var updatedNotifications = unreadNotifications.Select(notification =>
        {
            notification.ReadAt = updatedAt;
            return notification;
        });
        
        await repository.UpdateManyAsync(updatedNotifications);
    }
}
