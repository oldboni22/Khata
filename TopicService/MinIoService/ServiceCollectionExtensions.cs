using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace MinIoService;

public static class ServiceCollectionExtensions
{
    
    public static IServiceCollection AddMinioService(this IServiceCollection services, Func<MinioServiceOptions> optionsFactory)
    {
        var options = optionsFactory();

        return services.AddMinio(config =>
        {
            config
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(false)
                .Build();
        });
    } 
}
