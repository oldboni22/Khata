using NotificationService.DAL.Contracts.Repos;
using NotificationService.Domain.Models;

namespace NotificationService.API.Services;

public interface INotificationService
{
    Task CreatePostNotificationsAsync(IEnumerable<Guid> userIds, Guid postId);
    
    Task CreateCommentNotificationAsync(Guid userId, Guid commentId);
}

public class NotificationService(IGenericRepository<NotificationBase> repository) : INotificationService
{
    public async Task CreatePostNotificationsAsync(IEnumerable<Guid> userIds, Guid postId)
    {
        var notifications = userIds
            .Select(uid => new PostNotification()
            {
                UserId = uid,
                PostId = postId
            });

        await repository.CreateManyAsync(notifications);
    }

    public async Task CreateCommentNotificationAsync(Guid userId, Guid commentId)
    {
        var notification = new CommentNotification()
        {
            UserId = userId,
            CommentId = commentId
        };
        
        await repository.CreateAsync(notification);
    }
}
