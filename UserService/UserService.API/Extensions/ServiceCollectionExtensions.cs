using System.Reflection;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Shared;
using UserService.API.ActionFilters;
using UserService.API.Utilities;
using UserService.BLL.Extensions;

namespace UserService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApiDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddBusinessLayerDependencies(configuration)
            .AddMapping()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
            .AddControllers();
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
    
    private static IServiceCollection AddMapping(this IServiceCollection services)
    {
        return services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
    }
}
