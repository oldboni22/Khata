using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.DAL.Models.Entities;

public class User : EntityBase
{
    [MaxLength(15)]
    public required string Name { get; set; }
    
    public List<UserTopicRelation> TopicStatuses { get; set; } = [];

    [MaxLength(50)]
    public required string Auth0Id { get; set; }
}
