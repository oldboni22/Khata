using System.Reflection;
using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NotificationService.API.Services;
using Shared;

namespace NotificationService.API.Extensions;

public static class ServiceCollectionExtensions
{
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
    
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        return 
            services
                .AddScoped<INotificationService, Services.NotificationService>()
                .AddMessageBroker(configuration);
    }
    
    private static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddMassTransit(config =>
        {
            config.AddConsumers(Assembly.GetExecutingAssembly());
            
            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration[ConfigurationKeys.RabbitMqHost]);
                
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}