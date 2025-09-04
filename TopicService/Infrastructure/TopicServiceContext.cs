using Domain.Entities;
using Domain.Entities.Interactions;
using Infrastructure.EntityConfigs;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class TopicServiceContext : DbContext
{
    public DbSet<TopicReadDto> Topics { get; init; }
    
    public DbSet<Post> Posts { get; init; }
    
    public DbSet<Comment> Comments { get; init; }
    
    public DbSet<CommentInteraction> CommentInteractions { get; init; }
    
    public DbSet<PostInteraction> PostInteractions { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        AddInterceptors(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTopic(modelBuilder);
        ConfigurePost(modelBuilder);
        ConfigureComment(modelBuilder);
    }

    private void ConfigureTopic(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new TopicEntityConfiguration());

        builder
            .Entity<TopicReadDto>()
            .HasMany<TopicReadDto>(topic => topic.SubTopics)
            .WithOne()
            .HasForeignKey(topic => topic.ParentTopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<TopicReadDto>()
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

    private void AddInterceptors(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new TimeStampsInterceptor());
    }
}
