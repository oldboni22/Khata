using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EntityConfigs;

public class CommentEntityConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        var interactionNavigation = builder.Metadata.FindNavigation(nameof(Comment.Interactions));
        interactionNavigation!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
