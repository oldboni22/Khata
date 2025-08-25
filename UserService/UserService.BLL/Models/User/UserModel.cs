using System.ComponentModel.DataAnnotations;
using UserService.DAL.Models.Entities;

namespace UserService.BLL.Models.User;

public class UserModel : ModelBase
{
    public required string Name { get; set; }
    
    public List<UserTopicRelationModel> TopicStatuses { get; init; } = [];
}
