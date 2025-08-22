namespace UserService.BLL.Models;

public abstract class ModelBase
{
    public Guid Id { get; init; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}