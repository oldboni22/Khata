using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.DAL.Models;

public class PostNotification : NotificationBase
{
    public Guid PostId { get; set; }
    
    public override NotificationType NotificationType => NotificationType.Post;
}
