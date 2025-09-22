namespace Domain.Contracts.MessageBroker;

public interface IMessageSender
{
    Task SendPostNotificationCreateMessagesAsync(Guid postId, IEnumerable<Guid> usersIds, CancellationToken cancellationToken = default);
    
    Task SendCommentNotificationCreateMessageAsync(Guid userId, Guid commentId, CancellationToken cancellationToken = default);
}
