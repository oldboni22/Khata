using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using UserService.DAL;
using UserService.DAL.CacheService;
using UserService.DAL.Models.Entities;
using UserService.IntegrationTests.Utils;

namespace UserService.IntegrationTests.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection GenericReplace<T>(this IServiceCollection services, T implementation)
        where T : class
    {
        services.RemoveAll<T>();
        
        return services.AddSingleton(implementation);
    }
    
    public static IServiceCollection ReplaceDependencies(this IServiceCollection services)
    {
        return services
            .ReplaceDbContext()
            .ReplaceAuthentication();
    }
    
    private static IServiceCollection ReplaceDbContext(this IServiceCollection services)
    {
        var dbContextOptionsDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<UserServiceContext>));

        if (dbContextOptionsDescriptor is not null)
        {
            services.Remove(dbContextOptionsDescriptor);
        }

        var contextConfigurationDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(IDbContextOptionsConfiguration<UserServiceContext>));

        if (contextConfigurationDescriptor is not null)
        {
            services.Remove(contextConfigurationDescriptor);
        }
        
        var dbContextDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(UserServiceContext));
        
        if (dbContextDescriptor is not null)
        {
            services.Remove(dbContextDescriptor);
        }
        
        return services
            .AddDbContext<UserServiceContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
    }

    private static IServiceCollection ReplaceCache(this IServiceCollection services)
    {
        services
            .RemoveAll<IDistributedCache>()
            .RemoveAll<IUserAuth0IdCacheService>()
            .RemoveAll<CacheServiceOptions>();

        return services
            .AddDistributedMemoryCache()
            .Configure<CacheServiceOptions>(options => options.Lifetime = "5")
            .AddSingleton<IUserAuth0IdCacheService, UserAuth0IdCacheService>();
            
    }
    
    private static IServiceCollection ReplaceAuthentication(this IServiceCollection services)
    {
        services.RemoveAll<IAuthenticationService>();
        services.RemoveAll<IAuthenticationHandler>();

        services
            .AddAuthentication(TestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.SchemeName, options => { });
        
        return services;
    }
}
