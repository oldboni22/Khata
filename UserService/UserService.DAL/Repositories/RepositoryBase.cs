using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UserService.DAL.Models.Entities;

namespace UserService.DAL.Repositories;

public interface IGenericRepository<T>
    where T : EntityBase
{
    Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> expression, bool trackChanges = false, CancellationToken cancellationToken = default);
    
    Task<T?> FindByIdAsync(Guid id, bool trackChanges = true, CancellationToken cancellationToken = default);
    
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    
    Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GenericRepository<T>(UserServiceContext context) : IGenericRepository<T> 
    where T : EntityBase
{
    protected UserServiceContext Context { get; } = context;

    public async Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> expression, bool trackChanges, CancellationToken cancellationToken = default)
    {
        var query = Context.Set<T>().Where(expression);
        query = trackChanges ? query : query.AsNoTracking();
        
        return await query.ToListAsync(cancellationToken);
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

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, false, cancellationToken);
        
        if(entity is null)
        {
            return false;
        }
        
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Set<T>().AnyAsync(ent => ent.Id == id, cancellationToken);
    }
}
