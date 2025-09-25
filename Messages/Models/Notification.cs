using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Messages.Models;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid EntityId { get; set; }

    [BsonRepresentation(BsonType.DateTime)]
    public DateTime CreatedAt { get; set; }

    [BsonRepresentation(BsonType.DateTime)]
    public DateTime? ReadAt { get; set; }

    public ParentEntity? Parent { get; set; }

    [BsonRepresentation(BsonType.String)]
    public EntityType EntityType { get; set; }
}
