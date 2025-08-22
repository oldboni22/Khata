using System.ComponentModel.DataAnnotations;
using UserService.DAL.Models.Entities;

namespace UserService.BLL.Models.User;

public class UserModel : ModelBase
{
    [MaxLength(15)]
    public required string Name { get; set; }
    
    public List<UserTopicRelation> TopicStatuses { get; set; } = [];
}