using Microsoft.EntityFrameworkCore;
using UserService.DAL.Models.Entities;

namespace UserService.DAL.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> FindUserByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default);
}

public class UserRepository(UserServiceContext context) : GenericRepository<User>(context), IUserRepository 
{
    public Task<User?> FindUserByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default)
    {
        return Context
            .Set<User>()
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Auth0Id == auth0Id, cancellationToken);
    }
}
