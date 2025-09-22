namespace NotificationService.Domain.Models;

public record CommentNotification : NotificationBase
{
    public Guid CommentId { get; set; }
    
    public override NotificationType NotificationType => NotificationType.Comment;
}
