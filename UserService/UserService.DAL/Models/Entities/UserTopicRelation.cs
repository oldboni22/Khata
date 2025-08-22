using UserService.DAL.Models.Enums;

namespace UserService.DAL.Models.Entities;

public class UserTopicRelation : EntityBase
{
    public Guid UserId { get; init; }
    
    public Guid TopicId { get; init; }

    public User User { get; init; } = null!;

    public UserTopicRelationStatus TopicRelationStatus { get; set; } = UserTopicRelationStatus.Subscribed;
}
