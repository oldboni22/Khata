using System.Linq.Expressions;
using Domain.Entities;
using Domain.Exceptions;
using Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.PagedList;

namespace Infrastructure;

public class GenericRepository<T>(TopicServiceContext context) : IGenericRepository<T> 
    where T : EntityBase
{
    protected TopicServiceContext Context { get; } = context;

    public async Task<PagedList<T>> FindByConditionAsync(
        Expression<Func<T, bool>> expression, 
        PaginationParameters paginationParameters, 
        bool trackChanges = false, 
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<T>()
            .Where(expression)
            .Skip((paginationParameters.PageNumber -1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize);
        
        query = trackChanges ? query : query.AsNoTracking();

        var list = await query.ToListAsync(cancellationToken);

        var pageCount = (int)Math.Ceiling(Context.Set<T>().Count()/(double)paginationParameters.PageSize);
        
        return list.ToPagedList(paginationParameters.PageNumber, paginationParameters.PageSize, pageCount);
    }

    public async Task<T?> FindByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<T>().Where(ent => ent.Id == id);
        query = trackChanges ? query : query.AsNoTracking();
        
        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<T>().AddAsync(entity, cancellationToken);
        
        await Context.SaveChangesAsync(cancellationToken);

        return entity;
    }
    
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, false, cancellationToken);

        if (entity is null)
        {
            return false;
        }
        
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
