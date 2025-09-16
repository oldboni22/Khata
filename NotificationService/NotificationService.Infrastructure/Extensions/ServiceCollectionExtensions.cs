using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.DAL.MangoService;

namespace NotificationService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNDataLayerDependenciesDependencies(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddMongoServices(configuration);
    }

    private static IServiceCollection AddMongoServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MangoServiceOptions>(configuration.GetSection(MangoServiceOptions.SectionName));

        return services;
    }
}
