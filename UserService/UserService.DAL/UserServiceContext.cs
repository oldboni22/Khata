using Microsoft.EntityFrameworkCore;
using UserService.DAL.Models.Entities;

namespace UserService.DAL;

public class UserServiceContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<UserTopicRelation> UserTopics { get; set; }

    public override int SaveChanges()
    {
        AddTimestamp();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTimestamp();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void AddTimestamp()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e is
            {
                Entity: EntityBase, 
                State: EntityState.Added or EntityState.Modified 
            });

        foreach (var entry in entries)
        {
            var entity =(EntityBase)entry.Entity; 
            
            entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entry.Property(nameof(EntityBase.CreatedAt)).IsModified = false;
            }
            
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUser(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Name)
            .IsUnique();
    }
}
