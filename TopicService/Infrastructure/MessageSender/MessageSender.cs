using Domain.Contracts.MessageBroker;
using MassTransit;
using Messages;
using Microsoft.Extensions.Options;

namespace Infrastructure.MessageSender;

public class MessageSender(ISendEndpointProvider provider, IOptions<MessageSenderOptions> options) : IMessageSender
{
    private readonly string _postQueueName = options.Value.PostQueueName;
    
    private readonly string _commentQueueName = options.Value.CommentQueueName;
    
    public async Task SendPostNotificationCreateMessagesAsync(
        Guid postId, IEnumerable<Guid> usersIds, CancellationToken cancellationToken = default)
    {
        var endpoint = await provider.GetSendEndpoint(new Uri($"queue:{_postQueueName}"));
        
        var message = new PostNotificationsCreateMessage(postId, usersIds);
        
        await endpoint.Send(message, cancellationToken);
    }

    public async Task SendCommentNotificationCreateMessageAsync(
        Guid userId, Guid commentId, CancellationToken cancellationToken = default)
    {
        var endpoint = await provider.GetSendEndpoint(new Uri($"queue:{_commentQueueName}"));
        
        var message = new CommentNotificationCreateMessage(userId, commentId);
        
        await endpoint.Send(message, cancellationToken);
    }
}
