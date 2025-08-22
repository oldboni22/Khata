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
    private readonly UserServiceContext _context = context;
    
    public async Task<List<T>> FindByConditionAsync(Expression<Func<T, bool>> expression, bool trackChanges)
    {
        var query = _context.Set<T>().Where(expression);
        query = trackChanges ? query : query.AsNoTracking();
        
        return await query.ToListAsync();
    }

    public async Task<T?> FindByIdAsync(Guid id, bool trackChanges)
    {
        var query = _context.Set<T>().Where(ent => ent.Id == id);
        query = trackChanges ? query : query.AsNoTracking();
        
        return await query.SingleOrDefaultAsync();
    }

    public async Task<T> CreateAsync(T entity)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync();
        
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        
        if(entity == null)
        {
            return false;
        }
        
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
        
        return true;
    }
}
