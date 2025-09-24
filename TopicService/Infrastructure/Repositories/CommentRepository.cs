using System.Linq.Expressions;
using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.PagedList;
using Shared.Search.Comment;

namespace Infrastructure.Repositories;

public class CommentRepository(TopicServiceContext context) : GenericReadOnlyRepository<Comment>(context), ICommentRepository
{
    public Task<PagedList<Comment>> FindByConditionAsync(
        Guid postId,
        PaginationParameters paginationParameters,
        CommentFilter? filter,
        (Expression<Func<Comment, object>> predicate, bool isAscending)[]? keySelectors = null,
        bool trackChanges = false,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Comment, bool>> predicate = c => c.PostId == postId;
        
        var filterExpression = ParseFilter(filter);
        if (filterExpression is not null)
        {
            predicate = predicate.And(filterExpression);
        }
        
        return FindByConditionAsync(predicate, paginationParameters, keySelectors, trackChanges, cancellationToken);
    }
    
    private Expression<Func<Comment, bool>>? ParseFilter(CommentFilter? filter)
    {
        if (filter is null)
        {
            return null;
        }
        
        Expression<Func<Comment, bool>>? predicate = c => true;
        
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            predicate = predicate
                .And(c => EF.Functions.ILike(c.Text, filter.SearchTerm.ToSearchString()));
        }
        
        return predicate;
    }
}
