using System.Linq.Expressions;
using Domain.Entities;
using Shared.PagedList;
using Shared.Search.Topic;

namespace Domain.Contracts.RepositoryContracts;

public interface ITopicRepository : IGenericRepository<Topic>
{
    Task<PagedList<Topic>> FindByConditionAsync(
        PaginationParameters paginationParameters,
        Guid? parentTopicId,
        TopicFilter? filter,    
        (Expression<Func<Topic, object>> predicate, bool isAscending)[]? keySelectors = null, 
        bool trackChanges = false, 
        CancellationToken cancellationToken = default);
    
    Task<bool> IsOwnerAsync(Guid topicId, Guid userId, CancellationToken cancellationToken = default);
    
    Task<Topic?> FindTopicWithPostsAsync(Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default);
    
    Task<Topic?> FindTopicWithPostsAndThenCommentsAsync(Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default);
    
    Task<Topic?> FindTopicWithSubTopicsAsync(Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default);
}
