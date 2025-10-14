using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Domain.Contracts;
using NotificationService.Infrastructure.GRpc;

namespace NotificationService.Infrastructure.Socket;

[Authorize]
public class NotificationHub(IUserGrpcService userGrpcService) : Hub
{
}
