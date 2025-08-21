using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.DAL.Models.Entities;

public class User : EntityBase
{
    [MaxLength(15)]
    public required string Name { get; set; }
    
    public List<UserTopicStatus> TopicStatuses { get; set; } = [];
    
}