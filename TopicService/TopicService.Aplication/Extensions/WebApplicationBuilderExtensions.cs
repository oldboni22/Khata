using Serilog;
using Shared;

namespace TopicService.API.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((_, sp, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(sp)
                .WriteTo.Console()
                .WriteTo.File(builder.Configuration[ConfigurationKeys.SerilogLogFile]!);
        });
    }
}
