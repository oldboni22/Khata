using Microsoft.EntityFrameworkCore;
using UserService.DAL.CacheService;
using UserService.DAL.Models.Entities;

namespace UserService.DAL.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> FindUserByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default);
}

public class UserRepository(UserServiceContext context, IUserAuth0IdCacheService userAuth0IdCacheService) 
    : GenericRepository<User>(context), IUserRepository 
{
    public new async Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var updatedUser = await base.UpdateAsync(user, cancellationToken);

        if (updatedUser is null)
        {
            return null;
        }
        
        await userAuth0IdCacheService.SetValueAsync(updatedUser);
        
        return updatedUser;
    }

    public async Task<User?> FindUserByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default)
    {
        var user = await userAuth0IdCacheService.TryGetValueAsync(auth0Id);

        if (user is not null)
        {
            return user;
        }

        user = await Context
                .Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Auth0Id == auth0Id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        await userAuth0IdCacheService.SetValueAsync(user);
            
        return user;
    }
}
