using UserService.DAL.Models.Enums;

namespace UserService.DAL.Models.Entities;

public class UserTopicStatus : EntityBase
{
    public Guid UserId { get; init; }
    
    public Guid TopicId { get; init; }

    public User User { get; init; } = null!;

    public UserTopicRelation TopicRelation { get; set; } = UserTopicRelation.Absent;

}