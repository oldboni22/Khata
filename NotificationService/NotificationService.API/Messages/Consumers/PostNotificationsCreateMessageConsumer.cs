using MassTransit;
using Messages;
using NotificationService.API.Services;

namespace NotificationService.API.Messages.Consumers;

public class PostNotificationsCreateMessageConsumer(INotificationService service) : IConsumer<PostNotificationsCreateMessage>
{
    public async Task Consume(ConsumeContext<PostNotificationsCreateMessage> context)
    {
        await service.CreatePostNotificationsAsync(context.Message.UsersIds, context.Message.PostId);
    }
}