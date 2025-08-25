using Shared.Enums;
using UserService.BLL.Models.User;

namespace UserService.BLL.Models;

public class UserTopicRelationModel : ModelBase
{
    public Guid UserId { get; init; }
    
    public Guid TopicId { get; init; }

    public UserModel User { get; set; } = null!;

    public UserTopicRelationStatus TopicRelationStatus { get; set; } = UserTopicRelationStatus.Subscribed;
}
