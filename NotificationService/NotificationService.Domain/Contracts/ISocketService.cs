namespace NotificationService.Domain.Contracts;

public interface ISocketService
{
    Task SendSignalToUserAsync(Guid userId);
}