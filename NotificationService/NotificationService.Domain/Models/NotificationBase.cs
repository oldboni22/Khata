using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Domain.Models;

[BsonKnownTypes(typeof(PostNotification), typeof(CommentNotification))]
public abstract record NotificationBase
{
    [BsonId]
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime PostedOn { get; set; }

    [BsonRepresentation(BsonType.String)]
    public abstract NotificationType NotificationType { get; }
}
