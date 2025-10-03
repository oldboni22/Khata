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
    public async Task<User?> FindUserByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default)
    {
        var user = await userAuth0IdCacheService.TryGetValueAsync(auth0Id) 
                   ?? await Context
            .Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Auth0Id == auth0Id, cancellationToken);

        return user;
    }
}
