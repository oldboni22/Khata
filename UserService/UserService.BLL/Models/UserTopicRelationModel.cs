using UserService.DAL.Models.Enums;

namespace UserService.BLL.Models;

public class UserTopicRelationModel : ModelBase
{
    public Guid UserId { get; init; }
    
    public Guid TopicId { get; init; }

    public UserModel User { get; init; } = null!;

    public UserTopicRelationStatus TopicRelationStatus { get; set; } = UserTopicRelationStatus.Subscribed;
}