using System.Linq.Expressions;
using Domain.Entities;
using Shared.PagedList;
using Shared.Search.Comment;
using Shared.Search.Post;

namespace Domain.Contracts.RepositoryContracts;

public interface ICommentRepository : IGenericReadOnlyRepository<Comment>
{
    Task<PagedList<Comment>> FindByConditionAsync(
        Guid postId,
        PaginationParameters paginationParameters,
        CommentFilter? filter,    
        (Expression<Func<Comment, object>> predicate, bool isAscending)[]? keySelectors = null, 
        bool trackChanges = false, 
        CancellationToken cancellationToken = default);
}
