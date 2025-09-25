using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.PagedList;
using UserService.DAL.Models.Entities;

namespace UserService.DAL.Repositories;

public interface IGenericRepository<T>
    where T : EntityBase
{
    Task<PagedList<T>> FindByConditionAsync(
        Expression<Func<T, bool>> expression, 
        PaginationParameters paginationParameters,
        bool trackChanges = false, 
        CancellationToken cancellationToken = default);

    Task<List<T>> FindAllByConditionAsync(Expression<Func<T, bool>> expression, bool trackChanges = false, CancellationToken cancellationToken = default);
    
    Task<T?> FindByIdAsync(Guid id, bool trackChanges = true, CancellationToken cancellationToken = default);
    
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    
    Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GenericRepository<T>(UserServiceContext context) : IGenericRepository<T> 
    where T : EntityBase
{
    protected UserServiceContext Context { get; } = context;

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
        
        var count = await query.CountAsync(cancellationToken);
        
        query = trackChanges ? query : query.AsNoTracking();

        var list = await query.ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageCount = (int)Math.Ceiling(totalCount / (double)paginationParameters.PageSize);
        
        return list.ToPagedList(paginationParameters.PageNumber, paginationParameters.PageSize, pageCount, count);
    }

    public async Task<List<T>> FindAllByConditionAsync(
        Expression<Func<T, bool>> expression, bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        return await Context
            .Set<T>()
            .Where(expression)
            .ToListAsync(cancellationToken);
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

    public async Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync(cancellationToken);

        await Context.Entry(entity).ReloadAsync(cancellationToken);
        
        return entity;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Context.Set<T>().FirstAsync(ent => ent.Id == id, cancellationToken);
        
        Context.Set<T>().Remove(entity);
        
        await Context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Set<T>().AnyAsync(ent => ent.Id == id, cancellationToken);
    }
}
