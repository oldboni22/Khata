using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Domain.Models;

public record PostNotification : NotificationBase
{
    [BsonRepresentation(BsonType.String)]
    public Guid PostId { get; set; }
    
    public override NotificationType NotificationType => NotificationType.Post;
}
