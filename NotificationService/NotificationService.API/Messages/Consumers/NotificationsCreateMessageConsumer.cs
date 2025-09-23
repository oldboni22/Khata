using MassTransit;
using Messages;
using NotificationService.API.Services;

namespace NotificationService.API.Messages.Consumers;

public class NotificationsCreateMessageConsumer(INotificationService service) : IConsumer<NotificationsCreateMessage>
{
    public async Task Consume(ConsumeContext<NotificationsCreateMessage> context)
    {
        await service.CreateNotificationsAsync(context.Message.Notifications);
    }
}