using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace NotificationService.Infrastructure.MemoryCache;

public sealed class UserIdMemoryCacheService(IMemoryCache cache, IOptionsSnapshot<MemoryCacheServiceOptions> options) :
    MemoryCacheService<string, Guid>(cache, options)
{
    protected override string OptionsName => MemoryCacheServiceOptions.UserIdMemoryCacheName;
    
    protected override string CreateKeyString(string key) => key;
}