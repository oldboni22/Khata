using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Domain.Models;

public record CommentNotification : NotificationBase
{
    [BsonRepresentation(BsonType.String)]
    public Guid CommentId { get; set; }
    
    public override NotificationType NotificationType => NotificationType.Comment;
}
