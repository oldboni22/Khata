using Shared.Enums;
using Shared.Exceptions;

namespace Domain.Entities.Interactions;

public class CommentInteraction : EntityBase
{
    public Guid CommentId { get; init; }
    
    public Guid UserId { get; init; }

    public InteractionType Rating { get; private set; }
    
    private CommentInteraction(Guid commentId, Guid userId, InteractionType rating)
    {
        CommentId = commentId;
        UserId = userId;
        Rating = rating;
    }

    public static CommentInteraction Create(Guid commentId, Guid userId, InteractionType rating) =>
        new CommentInteraction(commentId, userId, rating);
    
    public void SetRating(InteractionType rating, Guid senderId)
    {
        if (senderId != UserId)
        {
            throw new ForbiddenException();
        }
        
        Rating = rating;
    }
}
