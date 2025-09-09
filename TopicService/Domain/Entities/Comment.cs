using Domain.Entities.Interactions;
using Domain.Exceptions;
using Shared.Enums;
using Shared.Exceptions;

namespace Domain.Entities;

public class Comment : EntityWithTimestamps
{
    #region Consts
    
    private const int TextMaxLength = 500;
    
    private const int TextMinLength = 5;
        
    #endregion

    private readonly List<CommentInteraction> _interactions = [];

    public IReadOnlyCollection<CommentInteraction> Interactions => _interactions;
    
    public Guid PostId { get; init; }
    
    public Guid UserId { get; init; }
    
    public string Text { get; private set; }
    
    public int LikeCount { get; private set; }
    
    public int DislikeCount { get; private set; }

    private Comment(string text, Guid postId, Guid userId)
    {
        PostId = postId;
        UserId = userId;
        Text = text;
    }

    public static Comment Create(string text, Guid postId, Guid authorId)
    {
        ValidateText(text);

        return new Comment(text, postId, authorId);
    }

    public void SetText(string text, Guid senderId)
    {
        if (senderId == UserId)
        {
            throw new ForbiddenException(); 
        }
        
        ValidateText(text);

        Text = text;
    }

    public CommentInteraction AddInteraction(Guid userId, InteractionType rating)
    {
        if (userId == UserId)
        {
            throw new SelfInteractionException();
        }
        
        var interaction = CommentInteraction.Create(Id, userId, rating);
        
        _interactions.Add(interaction);

        UpdateInteractions(rating, false);

        return interaction;
    }

    public void RemoveInteraction(Guid interactionId, Guid senderId)
    {
        var interaction = _interactions.FirstOrDefault(x => x.Id == interactionId) 
                          ?? throw new EntityNotFoundException<CommentInteraction>(interactionId);
        
        if(senderId != interaction.UserId)
        {
            throw new ForbiddenException();
        }
        
        _interactions.Remove(interaction);
        
        UpdateInteractions(interaction.Rating, false);
    }
    
    private void UpdateInteractions(InteractionType type, bool wasAdded)
    {
        if (type is InteractionType.Like)
        {
            LikeCount = wasAdded ? LikeCount + 1 : LikeCount - 1;
        }
        else if (type is InteractionType.Dislike)
        {
            DislikeCount = wasAdded ? DislikeCount + 1 : DislikeCount - 1;
        }
    }
    
    private static void ValidateText(string text)
    {
        if(string.IsNullOrEmpty(text) || text.Length is < TextMinLength or > TextMaxLength)
        {
            throw new Exception(); //TODO Custom exception
        }
    }
}
