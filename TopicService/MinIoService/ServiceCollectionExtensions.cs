using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace MinIoService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetMinioVariables(
        this IServiceCollection services,
        string endpoint,
        string accessKey,
        string secretKey)
    {
        StaticVariables.Endpoint = endpoint;
        StaticVariables.AccessKey = accessKey;
        StaticVariables.SecretKey = secretKey;
        
        return services;
    }
    
    public static IServiceCollection AddMinioService(this IServiceCollection services)
    {
        return services.AddMinio(config =>
            {
                config
                    .WithEndpoint(StaticVariables.Endpoint)
                    .WithCredentials(StaticVariables.AccessKey, StaticVariables.SecretKey)
                    .WithSSL(false)
                    .Build();
            })
            .AddSingleton<IMinioService, MinioService>();
    } 
}
