using Infrastructure.gRpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.DAL.MangoService;
using NotificationService.Domain.Contracts;
using NotificationService.Domain.Contracts.Repos;
using NotificationService.Infrastructure.GRpc;
using NotificationService.Infrastructure.MemoryCache;
using NotificationService.Infrastructure.Repositories;
using Shared;

namespace NotificationService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddGRpc(configuration)
            .AddDataDependencies(configuration);
    }
    
    private static IServiceCollection AddDataDependencies(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddSingleton<TimeProvider>(TimeProvider.System)
            .AddMongoServices(configuration)
            .AddScoped<INotificationRepository, NotificationRepository>();
    }
    
    private static IServiceCollection AddMongoServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .Configure<MangoServiceOptions>(configuration.GetSection(MangoServiceOptions.SectionName));
    }
    
    private static IServiceCollection AddGRpc(this IServiceCollection services, IConfiguration configuration)
    {
        var userServiceGrpcAddress = new Uri(configuration[ConfigurationKeys.UserGRpcAddress]!);
        
        services.AddGrpc();
        
        services.AddGrpcClient<UserGRpcApi.UserGRpcApiClient>(options =>
        {
            options.Address = userServiceGrpcAddress;
        });
        
        services.AddScoped<IUserGrpcService, UserGRpcClient>();

        return services;
    }
}
