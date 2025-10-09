using UserService.BLL.Models;

namespace UserService.API.DTO;

public class UserReadDto : ReadDtoBase
{
    public required string Name { get; init; }
    
    public List<UserTopicRelationReadDto> TopicStatuses { get; init; } = [];
}
