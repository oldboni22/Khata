using Domain.Entities.Interactions;
using Shared.Enums;

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
    
    public Guid AuthorId { get; init; }
    
    public string Text { get; private set; }

    private Comment(string text, Guid postId, Guid authorId)
    {
        PostId = postId;
        AuthorId = authorId;
        Text = text;
    }

    public static Comment Create(string text, Guid postId, Guid authorId)
    {
        ValidateText(text);

        return new Comment(text, postId, authorId);
    }

    public void Update(string text)
    {
        ValidateText(text);

        Text = text;
    }

    public CommentInteraction AddInteraction(Guid userId, PublicationRating rating)
    {
        var integration = CommentInteraction.Create(Id, userId, rating);
        
        _interactions.Add(integration);

        return integration;
    }
    
    private static void ValidateText(string text)
    {
        if(string.IsNullOrEmpty(text) || text.Length is < TextMinLength or > TextMaxLength)
        {
            throw new Exception(); //TODO Custom exception
        }
    }
}
