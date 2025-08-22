using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UserService.DAL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddContext(configuration);

        return services;
    }

    private static IServiceCollection AddContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        
        services.AddDbContext<UserServiceContext>(options =>
            options.UseNpgsql(connectionString));
        
        return services;
    }
}
