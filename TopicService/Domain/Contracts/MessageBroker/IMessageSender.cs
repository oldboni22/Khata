using Messages.Models;

namespace Domain.Contracts.MessageBroker;

public interface IMessageSender
{
    Task SendNotificationsCreateMessagesAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    
}
