using MassTransit;
using Messages;
using NotificationService.API.Services;

namespace NotificationService.API.Messages.Consumers;

public class CommentNotificationCreateMessageConsumer(INotificationService service) : IConsumer<CommentNotificationCreateMessage>
{
    public async Task Consume(ConsumeContext<CommentNotificationCreateMessage> context)
    {
        await service.CreateCommentNotificationAsync(context.Message.UserId, context.Message.CommentId);
    }
}