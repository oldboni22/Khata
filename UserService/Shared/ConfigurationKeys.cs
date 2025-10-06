namespace Shared;

public static class ConfigurationKeys
{
    public const string Auth0ApiKey = "Auth0:APIKey";
    
    public const string Auth0Audience = "Auth0:Audience";

    public const string Auth0Domain = "Auth0:Domain";

    public const string UserGRpcAddress = "GRpc:UserApiAddress";
    
    public const string UserGRpcPort = "GRpc:UserApiPort";
    
    public const string TopicGRpcAddress = "GRpc:TopicApiAddress";
    
    public const string TopicGRpcPort = "GRpc:TopicApiPort";
    
    public const string MinioEndpoint = "Minio:Endpoint";
    
    public const string MinioAccessKey = "Minio:AccessKey";
    
    public const string MinioSecretKey = "Minio:SecretKey";

    public const string ApplicationPort = "ApplicationPort";

    public const string SerilogFile = "SerilogSettings:LogFile";

    public const string RedisConnection = "Redis:Connection";
}
