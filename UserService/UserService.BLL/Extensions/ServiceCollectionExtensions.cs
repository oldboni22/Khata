using Infrastructure.gRpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;
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
            .AddMapping()
            .AddGRpc(configuration)
            .AddServices();
        
        return services;
    }
    
    private static IServiceCollection AddGRpc(this IServiceCollection services, IConfiguration configuration)
    {
        var port = new Uri(configuration[ConfigurationKeys.TopicGRpcPort]!);
        
        services.AddGrpc();
        services.AddGrpcClient<TopicGRpcApi.TopicGRpcApiClient>(options =>
        {
            options.Address = port;
        });

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
        return services.AddScoped<IUserService, Services.UserService>();
    }
}
