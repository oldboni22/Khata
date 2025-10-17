namespace NotificationService.Infrastructure.MemoryCache;

public class MemoryCacheServiceOptions
{
    public const string ConfigurationSection = "InMemoryCache";

    public const string UserIdMemoryCacheName = "UserId";

    public int Lifetime { get; set; } = 60;
}
