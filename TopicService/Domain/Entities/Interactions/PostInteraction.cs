using Shared.Enums;

namespace Domain.Entities.Interactions;

public class PostInteraction : EntityBase
{
    public Guid PostId { get; init; }
    
    public Guid UserId { get; init; }

    public PublicationRating Rating { get; private set; }
    
    private PostInteraction(Guid postId, Guid userId, PublicationRating rating)
    {
        PostId = postId;
        UserId = userId;
        Rating = rating;
    }

    public static PostInteraction Create(Guid postId, Guid userId, PublicationRating rating) =>
        new PostInteraction(postId, userId, rating);
    
    public void Update(PublicationRating rating) => Rating = rating;
}
