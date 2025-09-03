using System.Linq.Expressions;
using Domain.Entities;
using Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Filters;
using Shared.PagedList;

namespace Infrastructure;

public class TopicRepository(TopicServiceContext context) : GenericRepository<Topic>(context), ITopicRepository
{
    public async Task<PagedList<Topic>> FindByConditionAsync(
        Expression<Func<Topic, bool>> expression,
        TopicSortOptions sortOptions,
        bool ascending,
        PaginationParameters paginationParameters,
        bool trackChanges = false,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Topic>()
            .Where(expression);

        query = sortOptions switch
        {
            TopicSortOptions.CreateDate => ascending?
                query.OrderBy(t => t.CreatedAt) :
                query.OrderByDescending(t => t.CreatedAt),
            
            TopicSortOptions.PostCount => ascending?
                query.OrderBy(t => t.Posts.Count) :
                query.OrderByDescending(t => t.Posts.Count),
            
            _ => query,
        };
        
        query = trackChanges ? query : query.AsNoTracking();

        var list = await query
            .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageCount = (int)Math.Ceiling(totalCount / (double)paginationParameters.PageSize);
        
        return list.ToPagedList(paginationParameters.PageNumber, paginationParameters.PageSize, pageCount);
    }
}
