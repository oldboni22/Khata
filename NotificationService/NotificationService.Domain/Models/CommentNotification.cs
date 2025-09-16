namespace NotificationService.Domain.Models;

public class CommentNotification : NotificationBase
{
    public Guid CommentId { get; set; }
    
    public override NotificationType NotificationType => NotificationType.Comment;
}
