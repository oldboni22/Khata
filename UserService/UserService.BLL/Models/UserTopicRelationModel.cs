using Shared.Enums;
using UserService.BLL.Models.User;

namespace UserService.BLL.Models;

public class UserTopicRelationModel : ModelBase
{
    public Guid UserId { get; init; }
    
    public Guid TopicId { get; init; }
    
    public required UserModel User { get; set; }
    
    public UserTopicRelationStatus TopicRelationStatus { get; set; } = UserTopicRelationStatus.Subscribed;
}
