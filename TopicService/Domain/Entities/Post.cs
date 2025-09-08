using Domain.Entities.Interactions;
using Domain.Exceptions;
using Microsoft.VisualBasic;
using Shared.Enums;
using Shared.Exceptions;

namespace Domain.Entities;

public class Post : EntityWithTimestamps
{
    #region Consts

    private const int TitleMaxLength = 30;

    private const int TitleMinLength = 5;

    private const int TextMaxLength = 1500;

    private const int TextMinLength = 5;

    #endregion

    private readonly List<Comment> _comments = [];

    private readonly List<PostInteraction> _interactions = [];

    public IReadOnlyCollection<Comment> Comments => _comments;

    public IReadOnlyCollection<PostInteraction> Interactions => _interactions;

    public Guid TopicId { get; init; }

    public Guid AuthorId { get; init; }

    public string Text { get; private set; }

    public string Title { get; init; }

    public int LikeCount { get; private set; }

    public int DislikeCount { get; private set; }

    private Post(string title, string text, Guid topicId, Guid authorId)
    {
        Title = title;
        TopicId = topicId;
        Text = text;
        AuthorId = authorId;
    }

    public static Post Create(string title, string text, Guid topicId, Guid authorId)
    {
        if (string.IsNullOrEmpty(title) || title.Length is < TitleMinLength or > TitleMaxLength)
        {
            throw new Exception(); //TODO Custom exception
        }

        ValidateText(text);

        return new Post(title, text, topicId, authorId);
    }

    public void SetText(string text)
    {
        ValidateText(text);

        Text = text;
    }

    public Comment AddComment(string text, Guid authorId)
    {
        var comment = Comment.Create(text, Id, authorId);

        _comments.Add(comment);

        return comment;
    }

    public void RemoveComment(Guid commentId, Guid senderId)
    {
        var comment = _comments.FirstOrDefault(x => x.Id == commentId)
                      ?? throw new EntityNotFoundException<Comment>(commentId);

        if (senderId != comment.UserId)
        {
            throw new ForbiddenException();
        }

        _comments.Remove(comment);
    }

    public PostInteraction AddInteraction(Guid userId, InteractionType rating)
    {
        if (userId == AuthorId)
        {
            throw new SelfInteractionException();
        }

        var integration = PostInteraction.Create(Id, userId, rating);

        _interactions.Add(integration);

        UpdateInteractions();

        return integration;
    }

    public void RemoveInteraction(Guid interactionId, Guid senderId)
    {
        var interaction = _interactions.FirstOrDefault(x => x.Id == interactionId)
                          ?? throw new EntityNotFoundException<PostInteraction>(interactionId);

        if (senderId != interaction.UserId)
        {
            throw new ForbiddenException();
        }

        _interactions.Remove(interaction);

        UpdateInteractions();
    }

    private void UpdateInteractions()
    {
        LikeCount = _interactions.Count(inter => inter.Rating is InteractionType.Like);
        DislikeCount = _interactions.Count - LikeCount;
    }
    
    private static void ValidateText(string text)
    {
        if (string.IsNullOrEmpty(text) || text.Length is < TextMinLength or > TextMaxLength)
        {
            throw new Exception(); //TODO Custom exception
        }
    }
}
