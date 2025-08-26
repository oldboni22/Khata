using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.DAL.Models.Entities;
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
        
        return services.AddDbContext<UserServiceContext>(options =>
            options.UseNpgsql(connectionString));
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
                .AddScoped<IUserRepository, UserRepository>()
                .AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
                .AddScoped<IUserTopicRelationRepository, UserTopicRelationRepository>();
    }
}
