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
            .AddValidatorsFromAssembly(typeof(Program).Assembly)
            .AddControllers();
    }

    private static IServiceCollection AddMapping(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        
        return services;
    }
}
