using Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;

namespace UserApi.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .WriteTo.Console()
                .WriteTo.File("log.txt");
        });
    }
    
}