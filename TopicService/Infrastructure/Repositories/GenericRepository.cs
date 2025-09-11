using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Domain.Exceptions;


namespace Infrastructure.Repositories;

public class GenericRepository<T>(TopicServiceContext context) : GenericReadOnlyRepository<T>(context), IGenericRepository<T>
    where T : EntityBase
{
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
    
    public async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        await Context.SaveChangesAsync(cancellationToken);
    }
}
