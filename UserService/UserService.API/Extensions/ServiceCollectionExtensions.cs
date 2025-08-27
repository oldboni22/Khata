using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

    public static void AddCorsPolicies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
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
