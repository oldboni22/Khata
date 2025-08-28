using System.Reflection;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
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
    
    public static IServiceCollection AddCorsPolicies(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddCors(options =>
        {
            options.AddPolicy("Auth0",
                builder =>
                {
                    builder.WithOrigins(configuration["Auth0:Domain"]!)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });
    }
    
    public static void AddAuthenticationBearer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Audience = configuration["Auth0:Audience"];
                options.Authority = configuration["Auth0:Domain"];
                
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
