namespace UserService.DAL.Models.Entities;

public abstract class EntityBase
{
    public Guid Id { get; init; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
}