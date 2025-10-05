using Infrastructure.gRpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using MinIoService;
using Shared;
using UserService.BLL.gRpc;
using UserService.BLL.Minio;
using UserService.BLL.Services;
using UserService.BLL.Utilities;
using UserService.DAL.Extensions;

namespace UserService.BLL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDataLayerDependencies(configuration)
            .AddMinio(configuration)
            .AddMapping()
            .AddGRpc(configuration)
            .AddServices();
        return services;
    }
    
    private static IServiceCollection AddMinio(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration[ConfigurationKeys.MinioEndpoint];
        var accessKey = configuration[ConfigurationKeys.MinioAccessKey];
        var secretKey = configuration[ConfigurationKeys.MinioSecretKey];

        return services
            .AddMinioService(options =>
            {
                options.AccessKey = accessKey!;
                options.SecretKey = secretKey!;
                options.Endpoint = endpoint!;
            })
            .AddSingleton<IMinioService, UserMinioService>();
    }
    
    private static IServiceCollection AddGRpc(this IServiceCollection services, IConfiguration configuration)
    {
        var topicServiceGrpcAddress = new Uri(configuration[ConfigurationKeys.TopicGRpcAddress]!);
        
        services.AddGrpc();
        
        services.AddGrpcClient<TopicGRpcApi.TopicGRpcApiClient>(options =>
        {
            options.Address = topicServiceGrpcAddress;
        });

        services.AddScoped<ITopicGRpcClient,TopicGRpcClientWrapper>();
        
        return services;
    }
    
    private static IServiceCollection AddMapping(this IServiceCollection services)
    {
        return services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services.AddScoped<IUserService, Services.UserRepository>();
    }
}
