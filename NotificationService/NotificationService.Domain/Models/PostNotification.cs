namespace NotificationService.Domain.Models;

public record PostNotification : NotificationBase
{
    public Guid PostId { get; set; }
    
    public override NotificationType NotificationType => NotificationType.Post;
}
