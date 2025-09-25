using System.Linq.Expressions;
using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.PagedList;
using Shared.Search.Topic;

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

    public Task<Topic?> FindTopicWithPostsAsync(
        Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default)
    {
        var query = Context
            .Topics
            .Include(t => t.Posts)
            .AsQueryable();
        
        query = trackChanges? query : query.AsNoTrackingWithIdentityResolution();
        
        return query.FirstOrDefaultAsync(t => t.Id == topicId, cancellationToken: cancellationToken);
    }

    public Task<Topic?> FindTopicWithPostsAndThenCommentsAsync(
        Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default)
    {
        var query = Context
            .Topics
            .AsSplitQuery()
            .Include(t => t.Posts)
            .ThenInclude(p => p.Comments)
            .AsQueryable();
        
        query = trackChanges? query : query.AsNoTrackingWithIdentityResolution();
        
        return query.FirstOrDefaultAsync(t => t.Id == topicId, cancellationToken: cancellationToken);
    }

    public Task<Topic?> FindTopicWithSubTopicsAsync(Guid topicId, bool trackChanges = true, CancellationToken cancellationToken = default)
    {
        var query = Context
            .Topics
            .Include(t => t.SubTopics)
            .AsQueryable();
        
        query = trackChanges? query : query.AsNoTrackingWithIdentityResolution();
        
        return query.FirstOrDefaultAsync(t => t.Id == topicId, cancellationToken: cancellationToken);
    }

    public async Task<PagedList<Topic>> FindByConditionAsync(
        PaginationParameters paginationParameters, 
        Guid? parentTopicId, 
        TopicFilter? filter,
        (Expression<Func<Topic, object>> predicate, 
        bool isAscending)[]? keySelectors = null, 
        bool trackChanges = false,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Topic, bool>> expression = parentTopicId is null? 
            t => t.ParentTopicId == null : 
            t => t.ParentTopicId == parentTopicId;
        
        var filterExpression = ParseFilter(filter);
        if (filterExpression is not null)
        {
            expression = expression.And(filterExpression);
        }
        
        return await FindByConditionAsync(expression, paginationParameters, keySelectors, trackChanges, cancellationToken);
    }
    
    private Expression<Func<Topic, bool>>? ParseFilter(TopicFilter? filter)
    {
        if (filter is null)
        {
            return null;
        }
        
        Expression<Func<Topic, bool>>? predicate = t => true;
        
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            predicate = predicate
                .And(t => EF.Functions.ILike(t.Name, filter.SearchTerm.ToSearchString()));
        }
        
        return predicate;
    }
}
