using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TopicRepository(TopicServiceContext context) : GenericRepository<Topic>(context), ITopicRepository
{
    public async Task<bool> IsOwnerAsync(Guid topicId, Guid userId, CancellationToken cancellationToken = default)
    {
        var topic = await Context
            .Topics
            .Select(t => new { t.Id, t.OwnerId })
            .FirstOrDefaultAsync(t => t.Id == topicId && t.OwnerId == userId, cancellationToken: cancellationToken);

        if (topic == null)
        {
            return false;
        }

        return topic.OwnerId == userId;
    }

    public Task<Topic?> FindTopicWithPostsAsync(Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default)
    {
        var query = Context
            .Topics
            .Include(t => t.Posts)
            .AsSplitQuery();
        
        query = trackChanges? query : query.AsNoTracking();
        
        return query.FirstOrDefaultAsync(t => t.Id == topicId, cancellationToken: cancellationToken);
    }

    public Task<Topic?> FindTopicWithSubTopicsAsync(Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default)
    {
        var query = Context
            .Topics
            .Include(t => t.SubTopics)
            .AsSplitQuery();
        
        query = trackChanges? query : query.AsNoTracking();
        
        return query.FirstOrDefaultAsync(t => t.Id == topicId, cancellationToken: cancellationToken);
    }
}
