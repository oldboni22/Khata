using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace NotificationService.Infrastructure.MemoryCache;

public interface IMemoryCacheService<in TKey ,TValue>
{
    void AddOrUpdate(TKey key, TValue value);
    
    TValue? GetValue(TKey key);
    
    void RemoveValue(TKey key);
}

public abstract class MemoryCacheService<TKey, TValue> : IMemoryCacheService<TKey ,TValue>
{
    protected IMemoryCache Cache;

    private readonly int _lifetime;
    
    protected MemoryCacheService(IMemoryCache cache, IOptionsSnapshot<MemoryCacheServiceOptions> options)
    {
        Cache = cache;

        _lifetime = options.Get(OptionsName).Lifetime;
    }
    
    public void AddOrUpdate(TKey key,TValue value)
    {
        var keyString = CreateKeyString(key);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_lifetime)
        };
        
        Cache.Set(keyString, value, options);
    }

    public TValue? GetValue(TKey key)
    {
        var keyString = CreateKeyString(key);
        
        return Cache.Get<TValue>(keyString);
    }

    public void RemoveValue(TKey key)
    {
        var keyString = CreateKeyString(key);
        
        Cache.Remove(keyString);
    }

    protected abstract string OptionsName { get; }
    
    protected abstract string CreateKeyString(TKey key);
}
