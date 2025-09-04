using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigs;

public class TopicEntityConfiguration : IEntityTypeConfiguration<TopicReadDto>
{
    public void Configure(EntityTypeBuilder<TopicReadDto> builder)
    {
        builder.ToTable("topics");
        
        var subtopicsNavigation = builder.Metadata.FindNavigation(nameof(TopicReadDto.SubTopics));
        subtopicsNavigation!.SetPropertyAccessMode(PropertyAccessMode.Field);
        
        var postsNavigation = builder.Metadata.FindNavigation(nameof(TopicReadDto.Posts));
        postsNavigation!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
