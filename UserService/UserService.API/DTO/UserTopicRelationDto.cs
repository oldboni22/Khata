using Shared.Enums;
using UserService.API.Utilities.Validation;

namespace UserService.API.DTO;

public class UserTopicRelationDto
{
    public Guid UserId { get; init; }
    
    public Guid TopicId { get; init; }

    public UserReadDto User { get; set; } = null!;

    public UserTopicRelationStatus TopicRelationStatus { get; set; } = UserTopicRelationStatus.Subscribed;
}
