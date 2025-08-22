using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.DAL.Extensions;

namespace UserService.BLL.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddBusinessLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataLayerDependencies(configuration);
    }
}