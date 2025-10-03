using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UserService.DAL.Models.Entities;

namespace UserService.DAL.CacheService;

public interface IUserAuth0IdCacheService : IGenericCacheService<User>
{
}

public class UserAuth0IdCacheService(IOptions<CacheServiceOptions> options)
    : GenericCacheService<User>(options), IUserAuth0IdCacheService
{
    protected override string Prefix => "U_auth0";
    
    protected override string CreateKey(User value) => value.Auth0Id;
}
