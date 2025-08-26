using UserService.BLL.Models;

namespace UserService.API.DTO;

public class UserReadDto
{
    public required string Name { get; init; }
    
     public List<UserTopicRelationDto> TopicStatuses { get; init; } = [];
}
