using Serilog;

namespace UserService.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((builder, sp, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(sp)
                .WriteTo.Console()
                .WriteTo.File("log.txt");
        });
    }
}