namespace NotificationService.Infrastructure.GRpc;

public interface IUserGrpcService
{
    Task<Guid?> GetUserIdAsync(string auth0Id);
}
