using Domain.Contracts.GRpc;
using Domain.Contracts.MessageBroker;
using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Infrastructure.gRpc;
using Infrastructure.MessageSender;
using Infrastructure.Minio;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinIoService;
using Shared;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        return
            services
                .AddTopicContext(configuration)
                .AddRepositories()
                .AddMinio(configuration)
                .AddSingleton<IMinioService, TopicMinioService>()
                .AddGRpc(configuration)
                .AddMessageBroker(configuration);
    }

    private static IServiceCollection AddMinio(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration[ConfigurationKeys.MinioEndpoint];
        var accessKey = configuration[ConfigurationKeys.MinioAccessKey];
        var secretKey = configuration[ConfigurationKeys.MinioSecretKey];

        return services
            .AddMinioService(options =>
            {
                options.AccessKey = accessKey!;
                options.SecretKey = secretKey!;
                options.Endpoint = endpoint!;
            });
        
    }
    
    private static IServiceCollection AddTopicContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        
        return services.AddDbContext<TopicServiceContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<ITopicRepository, TopicRepository>()
            .AddScoped<IGenericReadOnlyRepository<Post>, GenericReadOnlyRepository<Post>>()
            .AddScoped<IGenericReadOnlyRepository<Comment>, GenericReadOnlyRepository<Comment>>();
    }
    
    private static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddMassTransit(config =>
        {
            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration[ConfigurationKeys.RabbitMqHost]);
            });
            
        })
        .Configure<MessageSenderOptions>(configuration.GetSection(MessageSenderOptions.SectionName))
        .AddTransient<IMessageSender, MessageSender.MessageSender>();
    }
    
    private static IServiceCollection AddGRpc(this IServiceCollection services, IConfiguration configuration)
    {
        var userServiceGrpcAddress = new Uri(configuration[ConfigurationKeys.UserGRpcAddress]!);
        
        services.AddGrpc();
        
        services.AddGrpcClient<UserGRpcApi.UserGRpcApiClient>(options =>
        {
            options.Address = userServiceGrpcAddress;
        });
        
        services.AddScoped<IUserGRpcClient, UserGRpcClientWrapper>();

        return services;
    }
}
