using Domain.Entities;
using Domain.RepositoryContracts;

namespace Infrastructure;

public class TopicRepository(TopicServiceContext context) : GenericRepository<Topic>(context), ITopicRepository
{
    public async Task<bool> DoesUserOwnTopic(Guid topicId, Guid userId, CancellationToken cancellationToken = default)
    {
        var topic = await FindByIdAsync(topicId, false, cancellationToken);
        
        return topic?.OwnerId == userId;
    }
}
