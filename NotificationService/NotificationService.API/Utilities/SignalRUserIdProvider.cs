using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Contracts;
using NotificationService.Infrastructure.MemoryCache;
using Shared.Exceptions;
using Shared.Extensions;

namespace NotificationService.API.Utilities;

public class SignalRUserIdProvider(IMemoryCacheService<string, Guid> userIdMemoryCache) : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var userAuth0Id = connection.User.GetAuth0Id();

        var userId = userIdMemoryCache.GetValue(userAuth0Id!);

        if (userId == Guid.Empty)
        {
            throw new NotFoundException();
        }

        return userId.ToString();
    }
}