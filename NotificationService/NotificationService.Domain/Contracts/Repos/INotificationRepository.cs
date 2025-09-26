using Messages.Models;
using Shared.PagedList;

namespace NotificationService.Domain.Contracts.Repos;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<PagedList<Notification>> FindAllNotificationsAsync(
        Guid userId, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);
    
    Task<PagedList<Notification>> FindUnreadNotificationsAsync(
        Guid userId, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Notification>> FindUnreadNotificationsAsync(
        Guid userId, CancellationToken cancellationToken = default);
}
