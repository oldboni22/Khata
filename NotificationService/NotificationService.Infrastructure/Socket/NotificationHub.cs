using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Contracts;
using NotificationService.Infrastructure.GRpc;
using NotificationService.Infrastructure.MemoryCache;
using Shared.Exceptions;
using Shared.Extensions;

namespace NotificationService.Infrastructure.Socket;

[Authorize]
public class NotificationHub(IUserGrpcService userGrpcService, IMemoryCacheService<Guid, string> userIdCache) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User is null)
        {
            throw new NoClaimPrincipalException();
        }
        
        var auth0Id = Context.User.GetAuth0Id();
        
        var userId = await userGrpcService.GetUserIdAsync(auth0Id!);

        if (!userId.HasValue)
        {
            throw new NotFoundException();
        }
        
        userIdCache.AddOrUpdate(userId.Value, auth0Id!);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User is null)
        {
            throw new NoClaimPrincipalException();
        }
        
        var auth0Id = Context.User.GetAuth0Id();
        
        var userId = await userGrpcService.GetUserIdAsync(auth0Id!);

        if (!userId.HasValue)
        {
            throw new NotFoundException();
        }
        
        userIdCache.RemoveValue(userId.Value);
    }
}
