using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Contracts;
using NotificationService.Infrastructure.MemoryCache;
using Shared.Exceptions;
using Shared.Extensions;

namespace NotificationService.API.Utilities;

public class SignalRUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User.GetAuth0Id();
    }
}