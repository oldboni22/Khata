using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TopicRepository(TopicServiceContext context) : GenericRepository<Topic>(context), ITopicRepository
{
    public async Task<bool> IsOwnerAsync(Guid topicId, Guid userId, CancellationToken cancellationToken = default)
    {
        var topic = await context
            .Topics
            .Select(t => new { t.Id, t.OwnerId })
            .FirstOrDefaultAsync(t => t.Id == topicId && t.OwnerId == userId, cancellationToken: cancellationToken);

        if (topic == null)
        {
            return false;
        }

        return topic.OwnerId == userId;
    }
}
