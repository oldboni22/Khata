using Shared.Enums;
using UserService.API.Utilities.Validation;

namespace UserService.API.DTO;

public class UserTopicRelationReadDto : ReadDtoBase
{
    public Guid UserId { get; init; }
    
    public Guid TopicId { get; init; }

    public required UserReadDto User { get; set; }

    public UserTopicRelationStatus TopicRelationStatus { get; set; } = UserTopicRelationStatus.Subscribed;
}
