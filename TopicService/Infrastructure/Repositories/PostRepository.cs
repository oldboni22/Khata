using System.Linq.Expressions;
using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.PagedList;
using Shared.Search.Post;

namespace Infrastructure.Repositories;

public class PostRepository(TopicServiceContext context) : GenericRepository<Post>(context), IPostRepository
{
    public async Task<PagedList<Post>> FindByConditionAsync(
        Guid topicId,
        PaginationParameters paginationParameters,
        PostFilter filter,
        (Expression<Func<Post, object>> predicate, bool isAscending)[]? keySelectors = null, 
        bool trackChanges = false,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Post, bool>> predicate = p => p.TopicId == topicId;
        
        var filterExpression = ParseFilter(filter);
        if (filterExpression is not null)
        {
            predicate = predicate.And(filterExpression);
        }

        return await FindByConditionAsync(predicate, paginationParameters, keySelectors, trackChanges, cancellationToken);
    }
    
    private static Expression<Func<Post, bool>>? ParseFilter(PostFilter? filter)
    {
        if (filter is null)
        {
            return null;
        }
        
        Expression<Func<Post, bool>> predicate = p => true;
        
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            predicate = predicate
                .And(p => EF.Functions.ILike(p.Title, filter.SearchTerm.ToSearchString()));
        }

        if (filter.MinLikes > 0)
        {
            predicate = predicate.And(p => p.LikeCount >= filter.MinLikes);
        }

        if (filter.UserId is not null)
        {
            predicate = predicate.And(p => p.AuthorId == filter.UserId);
        }
        
        return predicate;
    }
}
