using Shared.Enums;

namespace Domain.Entities.Interactions;

public class PostInteraction : EntityBase
{
    public Guid PostId { get; init; }
    
    public Guid UserId { get; init; }

    public InteractionType Rating { get; private set; }
    
    private PostInteraction(Guid postId, Guid userId, InteractionType rating)
    {
        PostId = postId;
        UserId = userId;
        Rating = rating;
    }

    public static PostInteraction Create(Guid postId, Guid userId, InteractionType rating) =>
        new PostInteraction(postId, userId, rating);
    
    public void SetRating(InteractionType rating) => Rating = rating;
}
