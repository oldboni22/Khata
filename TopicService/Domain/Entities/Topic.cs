using Domain.Exceptions;
using Shared.Exceptions;

namespace Domain.Entities;

public class Topic : EntityWithTimestamps
{
    #region Consts
    
    private const int NameMaxLength = 25;
    
    private const int NameMinLength = 5;
        
    #endregion
    
    private readonly List<Topic> _subTopics = [];

    private readonly List<Post> _posts = [];
    
    public IReadOnlyCollection<Topic> SubTopics => _subTopics;
    
    public IReadOnlyCollection<Post> Posts => _posts;
    
    public string Name { get; init; }

    public Guid? ParentTopicId { get; init; }

    public Guid OwnerId { get; private set; }
    
    public int PostCount { get; private set; }
    
    private Topic(string name, Guid ownerId, Guid? parentTopicId = null)
    {
        Name = name;
        ParentTopicId = parentTopicId;
        OwnerId = ownerId;
    }
    
    public static Topic Create(string name, Guid creatorId, Guid? parentTopic = null)
    {
        if (string.IsNullOrEmpty(name) || name.Length is < NameMinLength or > NameMaxLength)
        {
            throw new ArgumentException(); //TODO Custom exception
        }

        return new Topic(name, creatorId, parentTopic);
    }

    public Topic AddSubTopic(string subTopicName, Guid creatorId)
    {
        var subTopic = Create(subTopicName, creatorId, Id);
        
        _subTopics.Add(subTopic);

        return subTopic;
    }

    public void RemoveSubTopic(Guid subTopicId, Guid senderId, bool isMod)
    {
        var subTopic = _subTopics.FirstOrDefault(p => p.Id == subTopicId)
                       ?? throw new EntityNotFoundException<Topic>(subTopicId);

        if (senderId != OwnerId && senderId != subTopic.OwnerId && !isMod)
        {
            throw new ForbiddenException();
        }
        
        _subTopics.Remove(subTopic);
    }

    public Post AddPost(string title, string text, Guid authorId)
    {
        var post = Post.Create(title, text, Id, authorId);
        
        _posts.Add(post);
        PostCount++;

        return post;
    }

    public void RemovePost(Guid postId, Guid senderId, bool isMod)
    {
        var post = _posts.FirstOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);

        if (senderId != OwnerId && senderId != post.AuthorId && !isMod)
        {
            throw new ForbiddenException();
        }

        _posts.Remove(post);
        PostCount--;
    }

    public void SetOwner(Guid ownerId) => OwnerId = ownerId;
}
