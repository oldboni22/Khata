using Messages.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using NotificationService.DAL.MangoService;
using NotificationService.Domain.Contracts.Repos;
using NotificationService.Infrastructure.MangoService;

namespace NotificationService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddDataLayerDependencies(configuration);
    }
    
    private static IServiceCollection AddDataLayerDependencies(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddMongoServices(configuration)
            .AddScoped<IGenericRepository<Notification>, GenericRepository<Notification>>();
    }

    private static IServiceCollection AddMongoServices(this IServiceCollection services, IConfiguration configuration)
    {
        
        
        return services
            .Configure<MangoServiceOptions>(configuration.GetSection(MangoServiceOptions.SectionName));
    }
}
