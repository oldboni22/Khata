using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.DAL.Models;

[BsonKnownTypes(typeof(PostNotification), typeof(CommentNotification))]
public abstract class NotificationBase
{
    [BsonId]
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime PostedOn { get; set; }

    public abstract NotificationType GetNotificationType();
}
