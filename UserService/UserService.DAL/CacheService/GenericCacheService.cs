using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace UserService.DAL.CacheService;

public interface IGenericCacheService<T> where T : class
{
    Task SetValueAsync(T value);
    
    Task<T?> TryGetValueAsync(string id);
}

public abstract class GenericCacheService<T>(IOptions<CacheServiceOptions> options, IDistributedCache cache)
    : IGenericCacheService<T> where T : class
{
    protected abstract string Prefix { get; }

    protected IDistributedCache Cache { get; } = cache;

    protected int Lifetime { get; } = Convert.ToInt32(options.Value.Lifetime);
    
    public async Task SetValueAsync(T value)
    {
        var entryOptions = new DistributedCacheEntryOptions();
        entryOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(Lifetime));
        
        var key = $"{Prefix}_{CreateKey(value)}";
        
        var userJson = JsonSerializer.Serialize(value);
        
        await Cache.SetStringAsync(key,userJson, entryOptions);
    }

    public async Task<T?> TryGetValueAsync(string id)
    {
        var key = $"{Prefix}_{id}";
        
        var cacheValue = await Cache.GetStringAsync(key);

        if (string.IsNullOrEmpty(cacheValue))
        {
            return null;
        }
        
        return JsonSerializer.Deserialize<T>(cacheValue!.ToString());
    }

    protected abstract string CreateKey(T value);
}
