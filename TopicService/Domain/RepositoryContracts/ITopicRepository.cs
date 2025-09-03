using System.Linq.Expressions;
using Domain.Entities;
using Shared.Filters;
using Shared.PagedList;

namespace Domain.RepositoryContracts;

public interface ITopicRepository : IGenericRepository<Topic>
{
    Task<PagedList<Topic>> FindByConditionAsync(
    Expression<Func<Topic, bool>> expression,
    TopicSortOptions  sortOptions,
    bool ascending,
    PaginationParameters paginationParameters,
    bool trackChanges = false, 
    CancellationToken cancellationToken = default);
}
