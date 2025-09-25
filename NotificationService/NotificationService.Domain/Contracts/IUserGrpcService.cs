using System.Security.Claims;

namespace NotificationService.Domain.Contracts;

public interface IUserGrpcService
{
    Task<Guid?> GetUserIdAsync(string auth0Id);
}