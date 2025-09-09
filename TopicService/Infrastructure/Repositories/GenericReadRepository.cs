using System.Linq.Expressions;
using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.PagedList;

namespace Infrastructure.Repositories;

public class GenericReadOnlyRepository<T>(TopicServiceContext context) : IGenericReadOnlyRepository<T> where T : EntityBase
{
    protected TopicServiceContext Context { get; } = context;
    
    public async Task<PagedList<T>> FindByConditionAsync(
        Expression<Func<T, bool>> expression,
        PaginationParameters paginationParameters,
        (Expression<Func<T, object>> predicate, bool isAscending)[]? keySelectors = null, 
        bool trackChanges = false, 
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<T>()
            .Where(expression);

        var totalCount = await query.CountAsync(cancellationToken);
        
        if(keySelectors is not null)
        {
            query = ApplyKeySelectors(query, keySelectors);
        }
        
        query = trackChanges ? query : query.AsNoTracking();

        var list = await query
            .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize)
            .ToListAsync(cancellationToken);
        
        var pageCount = (int)Math.Ceiling(totalCount / (double)paginationParameters.PageSize);
        
        return list.ToPagedList(paginationParameters.PageNumber, paginationParameters.PageSize, pageCount);
    }

    public async Task<T?> FindByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<T>().Where(ent => ent.Id == id);
        query = trackChanges ? query : query.AsNoTracking();
        
        return await query.SingleOrDefaultAsync(cancellationToken);
    }
    
    private IQueryable<T> ApplyKeySelectors(
        IQueryable<T> query, (Expression<Func<T, object>> predicate, bool isAscending)[] selectors)
    {
        var ordered = ApplyPrimarySelector(query, selectors[0]);
        
        for (int i = 1; i < selectors.Length; i++)
        {
            ordered = ApplySelector(ordered, selectors[i]);
        }
        
        return ordered;
    }

    private IOrderedQueryable<T> ApplyPrimarySelector(
        IQueryable<T> query ,(Expression<Func<T, object>> predicate, bool isAscending) selector)
    {
        return selector.isAscending ?
            query.OrderBy(selector.predicate) : 
            query.OrderByDescending(selector.predicate);
    }
    
    private IOrderedQueryable<T> ApplySelector(
        IOrderedQueryable<T> query ,(Expression<Func<T, object>> predicate, bool isAscending) selector)
    {
        return selector.isAscending ?
            query.ThenBy(selector.predicate) : 
            query.ThenByDescending(selector.predicate);
    }
}
