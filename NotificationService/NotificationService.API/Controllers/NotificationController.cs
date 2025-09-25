using Messages.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.API.Services;
using Shared.Extensions;
using Shared.PagedList;

namespace NotificationService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<PagedList<Notification>> FindNotificationsAsync(
        [FromBody] PaginationParameters? paginationParameters, CancellationToken cancellationToken, [FromRoute] bool includeRead = false)
    {
        var senderId = User.GetAuth0Id();
        
        if (includeRead)
        {
            return await notificationService.FindAllNotificationsAsync(senderId!, paginationParameters, cancellationToken);
        }
        
        return await notificationService.FindUnreadNotificationsAsync(senderId!, paginationParameters, cancellationToken);
    }

    [HttpPost]
    public async Task MarkNotificationAsReadAsync([FromRoute] Guid notificationId, CancellationToken cancellationToken)
    {
        await notificationService.MarkNotificationAsReadAsync(notificationId, cancellationToken);
    }

    [HttpPost]
    public async Task MarkUnreadNotificationAsReadAsync(CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();
        
        await notificationService.MarkUnreadNotificationsAsReadAsync(senderId!, cancellationToken);
    }
    
}