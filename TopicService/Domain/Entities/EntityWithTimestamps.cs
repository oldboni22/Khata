namespace Domain.Entities;

public class EntityWithTimestamps : EntityBase
{
    public DateTime UpdatedAt { get; set; } 
    
    public DateTime CreatedAt { get; set; }
}
