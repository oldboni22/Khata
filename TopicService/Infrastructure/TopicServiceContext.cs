using Domain.Entities;
using Domain.Entities.Interactions;
using Infrastructure.EntityConfigs;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class TopicServiceContext : DbContext
{
    public DbSet<Topic> Topics { get; init; }
    
    public DbSet<Post> Posts { get; init; }
    
    public DbSet<Comment> Comments { get; init; }
    
    public DbSet<CommentInteraction> CommentInteractions { get; init; }
    
    public DbSet<PostInteraction> PostInteractions { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTopic(modelBuilder);
        ConfigurePost(modelBuilder);
        ConfigureComment(modelBuilder);
    }
    
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
                Entity: EntityWithTimestamps, 
                State: EntityState.Added or EntityState.Modified 
            });

        foreach (var entry in entries)
        {
            var entity = (EntityWithTimestamps)entry.Entity; 
            
            entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entry.Property(nameof(EntityWithTimestamps.CreatedAt)).IsModified = false;
            }
        }
    }

    private void ConfigureTopic(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new TopicEntityConfiguration());

        builder
            .Entity<Topic>()
            .HasMany<Topic>(topic => topic.SubTopics)
            .WithOne()
            .HasForeignKey(topic => topic.ParentTopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<Topic>()
            .HasMany<Post>(topic => topic.Posts)
            .WithOne()
            .HasForeignKey(post => post.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigurePost(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new PostEntityConfiguration());

        builder
            .Entity<Post>()
            .HasMany<Comment>(post => post.Comments)
            .WithOne()
            .HasForeignKey(comm => comm.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<Post>()
            .HasMany<PostInteraction>(post => post.Interactions)
            .WithOne()
            .HasForeignKey(interaction => interaction.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
    private void ConfigureComment(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new CommentEntityConfiguration());

        builder
            .Entity<Comment>()
            .HasMany(comm => comm.Interactions)
            .WithOne()
            .HasForeignKey(interaction => interaction.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
