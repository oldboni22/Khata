using System.Reflection;
using MassTransit;
using Shared;

namespace NotificationService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        return 
            services
                .AddScoped<Services.INotificationService, Services.NotificationService>()
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