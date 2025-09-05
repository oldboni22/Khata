using Domain.Contracts.GRpc;
using Domain.Contracts.RepositoryContracts;
using Infrastructure.gRpc;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace Infrastructure.Extensions;

public static class ServiceCollectinExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        return
            services
                .AddTopicContext(configuration)
                .AddRepositories()
                .AddGRpc(configuration);
    }

    private static IServiceCollection AddTopicContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        
        return services.AddDbContext<TopicServiceContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services.AddScoped<ITopicRepository, TopicRepository>();
    }
    
    private static IServiceCollection AddGRpc(this IServiceCollection services, IConfiguration configuration)
    {
        var port = new Uri(configuration[ConfigurationKeys.UserGRpcPort]!);
        
        services.AddGrpc();
        services.AddGrpcClient<UserGRpcApi.UserGRpcApiClient>(options =>
        {
            options.Address = port;
        });
        
        services.AddScoped<IUserGRpcClient, UserGRpcClientWrapper>();

        return services;
    }
}
