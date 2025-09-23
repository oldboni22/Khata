using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Messages.Models;

public class ParentEntity
{
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public ParentEntity? Parent { get; set; }

    public EntityType Type { get; set; }
}
