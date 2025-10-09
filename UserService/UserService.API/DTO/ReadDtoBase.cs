namespace UserService.API.DTO;

public class ReadDtoBase
{
    public Guid Id { get; init; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
