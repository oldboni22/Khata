using Domain.Contracts.MessageBroker;
using MassTransit;
using Messages;
using Messages.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.MessageSender;

public class MessageSender(ISendEndpointProvider provider, IOptions<MessageSenderOptions> options) : IMessageSender
{
    private readonly string _queueName =  options.Value.QueueName;
    
    public async Task SendNotificationsCreateMessagesAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
    {
        var endpoint = await provider.GetSendEndpoint(new Uri($"queue:{_queueName}"));
        
        var message = new NotificationsCreateMessage(notifications);
        
        await endpoint.Send(message, cancellationToken);
    }
}
