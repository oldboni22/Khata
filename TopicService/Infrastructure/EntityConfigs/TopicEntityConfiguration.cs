using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigs;

public class TopicEntityConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("topics");
        
        var subtopicsNavigation = builder.Metadata.FindNavigation(nameof(Topic.SubTopics));
        subtopicsNavigation!.SetPropertyAccessMode(PropertyAccessMode.Field);
        
        var postsNavigation = builder.Metadata.FindNavigation(nameof(Topic.Posts));
        postsNavigation!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
