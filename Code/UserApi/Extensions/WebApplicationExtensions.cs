using Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace UserApi.Extensions;

public static class WebApplicationExtensions
{
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

                var details = new ExceptionDetails(exceptionHandlerFeature.Error.Message, context.Response.StatusCode);
                
                await context.Response.WriteAsync(details.ToString());
            }
            
            
        }));
    }
}