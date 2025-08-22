using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UserService.DAL.Models.Entities;

namespace UserService.DAL.Repositories;

public interface IGenericRepository<T>
    where T : EntityBase
{
    Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> expression, bool trackChanges = false, CancellationToken ctx = default);
    
    Task<T?> FindByIdAsync(Guid id, bool trackChanges = true, CancellationToken ctx = default);
    
    Task<T> CreateAsync(T entity, CancellationToken ctx = default);
    
    Task<T> UpdateAsync(T entity, CancellationToken ctx = default);
    
    Task<bool> DeleteAsync(Guid id, CancellationToken ctx = default);
}

public class GenericRepository<T>(UserServiceContext context) : IGenericRepository<T> 
    where T : EntityBase
{
    protected readonly UserServiceContext Context = context;
    
    public async Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> expression, bool trackChanges, CancellationToken ctx = default)
    {
        var query = Context.Set<T>().Where(expression);
        query = trackChanges ? query : query.AsNoTracking();
        
        return await query.ToListAsync(ctx);
    }

    public async Task<T?> FindByIdAsync(Guid id, bool trackChanges, CancellationToken ctx = default)
    {
        var query = Context.Set<T>().Where(ent => ent.Id == id);
        query = trackChanges ? query : query.AsNoTracking();
        
        return await query.SingleOrDefaultAsync(ctx);
    }

    public async Task<T> CreateAsync(T entity, CancellationToken ctx = default)
    {
        Context.Set<T>().Add(entity);
        await Context.SaveChangesAsync(ctx);

        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken ctx = default)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync(ctx);
        
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ctx = default)
    {
        var entity = await FindByIdAsync(id, false, ctx);
        
        if(entity == null)
        {
            return false;
        }
        
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync(ctx);
        
        return true;
    }
}
