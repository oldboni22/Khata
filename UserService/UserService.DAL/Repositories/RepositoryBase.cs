using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UserService.DAL.Models.Entities;

namespace UserService.DAL.Repositories;

public interface IGenericRepository<T>
    where T : EntityBase
{
    Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> expression, bool trackChanges);
    
    Task<T?> FindByIdAsync(Guid id, bool trackChanges);
    
    Task<T> CreateAsync(T entity);
    
    Task<T> UpdateAsync(T entity);
    
    Task<bool> DeleteAsync(Guid id);
}

public class GenericRepository<T>(UserServiceContext context) : IGenericRepository<T> 
    where T : EntityBase
{
    protected readonly UserServiceContext Context = context;
    
    public async Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> expression, bool trackChanges)
    {
        var query = Context.Set<T>().Where(expression);
        query = trackChanges ? query : query.AsNoTracking();
        
        return await query.ToListAsync();
    }

    public async Task<T?> FindByIdAsync(Guid id, bool trackChanges)
    {
        var query = Context.Set<T>().Where(ent => ent.Id == id);
        query = trackChanges ? query : query.AsNoTracking();
        
        return await query.SingleOrDefaultAsync();
    }

    public async Task<T> CreateAsync(T entity)
    {
        Context.Set<T>().Add(entity);
        await Context.SaveChangesAsync();

        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync();
        
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await Context.Set<T>().FindAsync(id);
        
        if(entity == null)
        {
            return false;
        }
        
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync();
        
        return true;
    }
}
