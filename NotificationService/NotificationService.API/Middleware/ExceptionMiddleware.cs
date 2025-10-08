using NotificationService.API.Utilities;
using Shared.Exceptions;

namespace NotificationService.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, Serilog.ILogger logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if(exception.StackTrace is not null)
        {
            logger.Error(exception.StackTrace);
        }

        logger.Error(exception.Message);
        
        var details = exception switch
        {
            NotFoundException => new ExceptionDetails(exception.Message, 404),
            BadRequestException or _ => new ExceptionDetails(exception.Message, 400),
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = details.StatusCode;
        
        await context.Response.WriteAsJsonAsync(details);
    }
}
