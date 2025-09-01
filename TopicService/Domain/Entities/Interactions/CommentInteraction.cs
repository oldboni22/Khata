using Shared.Enums;

namespace Domain.Entities.Interactions;

public class CommentInteraction : EntityBase
{
    public Guid CommentId { get; init; }
    
    public Guid UserId { get; init; }

    public PublicationRating Rating { get; private set; }
    
    private CommentInteraction(Guid commentId, Guid userId, PublicationRating rating)
    {
        CommentId = commentId;
        UserId = userId;
        Rating = rating;
    }

    public static CommentInteraction Create(Guid commentId, Guid userId, PublicationRating rating) =>
        new CommentInteraction(commentId, userId, rating);
    
    public void Update(PublicationRating rating) => Rating = rating;
}
