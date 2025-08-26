using System.Reflection;
using FluentValidation;
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

    private static IServiceCollection AddMapping(this IServiceCollection services)
    {
        return services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
    }
}
