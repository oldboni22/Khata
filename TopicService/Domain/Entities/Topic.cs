using Domain.Exceptions;

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
    
    private Topic(string name, Guid creatorId, Guid? parentTopicId = null)
    {
        Name = name;
        ParentTopicId = parentTopicId;
        OwnerId = creatorId;
    }
    
    public static Topic Create(string name, Guid creatorId, Guid? parentTopic = null)
    {
        if (string.IsNullOrEmpty(name) || name.Length is < NameMinLength or > NameMaxLength)
        {
            throw new Exception(); //TODO Custom exception
        }

        return new Topic(name, creatorId, parentTopic);
    }

    public Topic AddSubTopic(string subTopicName, Guid creatorId)
    {
        var subTopic = Create(subTopicName, creatorId, Id);
        
        _subTopics.Add(subTopic);

        return subTopic;
    }

    public Post AddPost(string title, string text, Guid authorId)
    {
        var post = Post.Create(title, text, Id, authorId);
        
        _posts.Add(post);

        return post;
    }

    public void RemovePost(Guid postId, Guid senderId)
    {
        var post = _posts.FirstOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);

        _posts.Remove(post);
    }

    public void SetOwner(Guid ownerId) => OwnerId = ownerId;
}
