using Domain.Entities;

namespace Domain.Contracts.RepositoryContracts;

public interface ITopicRepository : IGenericRepository<Topic>
{
    Task<bool> IsOwnerAsync(Guid topicId, Guid userId, CancellationToken cancellationToken = default);
    
    Task<Topic?> FindTopicWithPostsAsync(Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default);
    
    Task<Topic?> FindTopicWithPostsAndThenCommentsAsync(Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default);
    
    Task<Topic?> FindTopicWithSubTopicsAsync(Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default);
}
