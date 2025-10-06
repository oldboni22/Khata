namespace UserService.DAL.CacheService;

public class CacheServiceOptions
{
    public const string ConfigurationSection = "Redis";

    public string Connection { get; set; } = string.Empty;
    
    public string Lifetime { get; set; } = string.Empty;
}
