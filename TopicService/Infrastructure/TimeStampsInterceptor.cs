using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure;

public class TimeStampsInterceptor : ISaveChangesInterceptor
{
    public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        return SetTimeStamps(eventData);
    }

    public ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return new ValueTask<int>(SetTimeStamps(eventData));
    }

    private int SetTimeStamps(DbContextEventData eventData)
    {
        var entities = eventData!.Context!.ChangeTracker
            .Entries()
            .Where(entry => entry is
            {
                Entity: EntityWithTimestamps,
                State: EntityState.Added or EntityState.Modified
            })
            .ToList();

        foreach (var entry in entities)
        {
            var entity = (EntityWithTimestamps)entry.Entity;
            
            entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
        
        return entities.Count;
    }
}
