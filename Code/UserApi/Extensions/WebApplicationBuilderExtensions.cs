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
    
    public static void AddExceptionMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler(builder => builder.Run(async context =>
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature != null)
            {
                context.Response.StatusCode = exceptionHandlerFeature.Error switch
                {
                    // Handle specific exceptions as they get added
                    
                    _ => StatusCodes.Status500InternalServerError,
                };
                
                
            }
            
            
        }));
    }
}