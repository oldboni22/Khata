using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace UserService.DAL.CacheService;

public interface IGenericCacheService<T> where T : class
{
    Task SetValueAsync(T value);
    
    Task<T?> TryGetValueAsync(string id);
}

public abstract class GenericCacheService<T> : IGenericCacheService<T> where T : class
{
    protected abstract string Prefix { get; }

    protected IDatabase Database { get; }

    protected int Lifetime { get; }

    protected GenericCacheService(IOptions<CacheServiceOptions> options)
    {
        var muxer = ConnectionMultiplexer.Connect(options.Value.Connection);

        if (!int.TryParse(options.Value.Lifetime, out var lifetime) || lifetime <= 0)
        {
            throw new ArgumentException("Lifetime must be a positive integer");
        }

        Lifetime = lifetime;
        
        Database = muxer.GetDatabase();
    }
    
    public async Task SetValueAsync(T value)
    {
        var key = $"{Prefix}_{CreateKey(value)}";
        
        var userJson = JsonSerializer.Serialize(value);
        
        await Database.StringSetAsync(key,userJson, TimeSpan.FromSeconds(Lifetime));
    }

    public async Task<T?> TryGetValueAsync(string id)
    {
        var key = $"{Prefix}_{id}";
        
        var redisValue = await Database.StringGetAsync(key);

        if (redisValue.IsNullOrEmpty)
        {
            return null;
        }
        
        return JsonSerializer.Deserialize<T>(redisValue!.ToString());
    }

    protected abstract string CreateKey(T value);
}
