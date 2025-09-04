using Domain.Exceptions;
using Shared.Exceptions;

namespace Domain.Entities;

public class TopicReadDto : EntityWithTimestamps
{
    #region Consts
    
    private const int NameMaxLength = 25;
    
    private const int NameMinLength = 5;
        
    #endregion
    
    private readonly List<TopicReadDto> _subTopics = [];

    private readonly List<Post> _posts = [];
    
    public IReadOnlyCollection<TopicReadDto> SubTopics => _subTopics;
    
    public IReadOnlyCollection<Post> Posts => _posts;

    public string Name { get; init; }

    public Guid? ParentTopicId { get; init; }

    public Guid OwnerId { get; private set; }
    
    private TopicReadDto(string name, Guid creatorId, Guid? parentTopicId = null)
    {
        Name = name;
        ParentTopicId = parentTopicId;
        OwnerId = creatorId;
    }
    
    public static TopicReadDto Create(string name, Guid creatorId, Guid? parentTopic = null)
    {
        if (string.IsNullOrEmpty(name) || name.Length is < NameMinLength or > NameMaxLength)
        {
            throw new Exception(); //TODO Custom exception
        }

        return new TopicReadDto(name, creatorId, parentTopic);
    }

    public TopicReadDto AddSubTopic(string subTopicName, Guid creatorId)
    {
        var subTopic = Create(subTopicName, creatorId, Id);
        
        _subTopics.Add(subTopic);

        return subTopic;
    }

    public void RemoveSubTopic(Guid subTopicId, Guid senderId)
    {
        var subTopic = _subTopics.FirstOrDefault(p => p.Id == subTopicId)
                       ?? throw new EntityNotFoundException<TopicReadDto>(subTopicId);

        if (senderId != OwnerId && senderId != subTopic.OwnerId)
        {
            throw new ForbiddenException();
        }
        
        _subTopics.Remove(subTopic);
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

        if (senderId != OwnerId && senderId != post.AuthorId)
        {
            throw new ForbiddenException();
        }

        _posts.Remove(post);
    }

    public void SetOwner(Guid ownerId) => OwnerId = ownerId;
}
