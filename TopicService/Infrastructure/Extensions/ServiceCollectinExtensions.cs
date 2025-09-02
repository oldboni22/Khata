using Infrastructure.gRpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace Infrastructure.Extensions;

public static class ServiceCollectinExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        return
            services.AddGRpc(configuration);
    }
    
    private static IServiceCollection AddGRpc(this IServiceCollection services, IConfiguration configuration)
    {
        var port = new Uri(configuration[ConfigurationKeys.GRpcPort]!);
        
        services.AddGrpc();
        services.AddGrpcClient<UserGRpcApi.UserGRpcApiClient>(options =>
        {
            options.Address = port;
        });

        return services;
    }
}
