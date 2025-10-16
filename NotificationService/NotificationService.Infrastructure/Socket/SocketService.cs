using NotificationService.Domain.Contracts;

namespace NotificationService.Infrastructure.Socket;

public interface ISocketService
{
    Task SendSignalToUserAsync(Guid userId);
}

public class SocketService : ISocketService
{
    public Task SendSignalToUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}
