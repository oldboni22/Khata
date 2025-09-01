using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigs;

public class PostEntityConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        var commentsNavigation = builder.Metadata.FindNavigation(nameof(Post.Comments));
        commentsNavigation!.SetPropertyAccessMode(PropertyAccessMode.Field);
        
        var interactionNavigation = builder.Metadata.FindNavigation(nameof(Post.Interactions));
        interactionNavigation!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
