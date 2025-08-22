using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.DAL.Repositories;

namespace UserService.DAL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddContext(configuration)
            .AddRepositories();

        return services;
    }

    private static IServiceCollection AddContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        
        services.AddDbContext<UserServiceContext>(options =>
            options.UseNpgsql(connectionString));
        
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserTopicRelationRepository, UserTopicRelationRepository>();
        
        return services;
    }
}
