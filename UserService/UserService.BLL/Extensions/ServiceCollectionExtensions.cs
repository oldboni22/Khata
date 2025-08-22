using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.BLL.Utilities;
using UserService.DAL.Extensions;

namespace UserService.BLL.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddBusinessLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDataLayerDependencies(configuration)
            .AddMapping();
    }
    
    private static IServiceCollection AddMapping(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        
        return services;
    }
}
