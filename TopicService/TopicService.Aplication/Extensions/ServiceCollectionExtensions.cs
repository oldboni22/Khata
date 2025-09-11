using System.Reflection;
using System.Security.Claims;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MinIoService;
using Shared;

namespace TopicService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerDependencies(this IServiceCollection collection, IConfiguration configuration)
    {
        return collection
            .AddMinio(configuration)
            .AddMapping()
            .AddInfrastructureDependencies(configuration);
    }

    private static IServiceCollection AddMinio(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration[ConfigurationKeys.MinioEndpoint];
        var accessKey = configuration[ConfigurationKeys.MinioAccessKey];
        var secretKey = configuration[ConfigurationKeys.MinioSecretKey];

        return services
            .AddMinioService(() => new  MinioServiceOptions(endpoint!, accessKey!, secretKey!));
    }
    
    private static IServiceCollection AddMapping(this IServiceCollection collection)
    {
        return collection.AddAutoMapper(cfg =>
        {
            
        }, Assembly.GetExecutingAssembly());
    }
    
    public static void AddAuthenticationBearer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Audience = configuration[ConfigurationKeys.Auth0Audience];
                options.Authority = configuration[ConfigurationKeys.Auth0Domain];
                
                options.TokenValidationParameters = new()
                {
                    NameClaimType = ClaimTypes.NameIdentifier,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                };
            });
    }
}
