using System.Reflection;
using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using NotificationService.API.Services;
using NotificationService.Domain.Contracts;
using NotificationService.Infrastructure.GRpc;
using NotificationService.Infrastructure.MemoryCache;
using Shared;
using Shared.Exceptions;
using Shared.Extensions;

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

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userIdMemoryCache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCacheService<Guid, string>>();
                        var userGRpcService = context.HttpContext.RequestServices.GetRequiredService<IUserGrpcService>();

                        if (context.Principal is null)
                        {
                            throw new BadRequestException();
                        }

                        var userAuth0Id = context.Principal.GetAuth0Id();

                        var userId = await userGRpcService.GetUserIdAsync(userAuth0Id!);

                        if (userId is null)
                        {
                            throw new NotFoundException();
                        }
                        
                        userIdMemoryCache.AddOrUpdate(userId.Value, userAuth0Id!);
                    }
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