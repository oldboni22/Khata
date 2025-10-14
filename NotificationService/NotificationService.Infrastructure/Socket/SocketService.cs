using NotificationService.Domain.Contracts;

namespace NotificationService.Infrastructure.Socket;

public class SocketService : ISocketService
{
    public Task SendSignalToUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}