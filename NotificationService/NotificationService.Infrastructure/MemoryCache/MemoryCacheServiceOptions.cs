namespace NotificationService.Infrastructure.MemoryCache;

public class MemoryCacheServiceOptions
{
    public const string ConfigurationSection = "InMemoryCache";

    public const string UserIdMemoryCacheName = "UserId";

    public string Lifetime { get; set; } = "60";
}