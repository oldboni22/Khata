namespace NotificationService.DAL.Models;

public class CommentNotification : NotificationBase
{
    public Guid CommentId { get; set; }
    
    public override NotificationType GetNotificationType() => NotificationType.Comment;
}
