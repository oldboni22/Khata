using System.Linq.Expressions;
using Domain.Entities;
using Shared.PagedList;
using Shared.Search.Post;

namespace Domain.Contracts.RepositoryContracts;

public interface IPostRepository : IGenericReadOnlyRepository<Post>
{
    Task<PagedList<Post>> FindByConditionAsync(
        Guid topicId,
        PaginationParameters paginationParameters,
        PostFilter? filter,    
        (Expression<Func<Post, object>> predicate, bool isAscending)[]? keySelectors = null, 
        bool trackChanges = false, 
        CancellationToken cancellationToken = default);
}
