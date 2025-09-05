using System.Reflection;
using Infrastructure.Extensions;

namespace TopicService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLayerDependencies(this IServiceCollection collection, IConfiguration configuration)
    {
        return collection
            .AddMapping()
            .AddInfrastructureDependencies(configuration);
    }

    private static IServiceCollection AddMapping(this IServiceCollection collection)
    {
        return collection.AddAutoMapper(cfg =>
        {
            
        }, Assembly.GetExecutingAssembly());
    }
}
