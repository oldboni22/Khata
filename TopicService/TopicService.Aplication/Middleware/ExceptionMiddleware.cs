using Shared.Exceptions;
using TopicService.API.Utilities;

namespace TopicService.API.Middleware;

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
        logger.Error(exception.Message);
        
        var details = exception switch
        {
            ForbiddenException => new ExceptionDetails(exception.Message, StatusCodes.Status403Forbidden),
            _ => new ExceptionDetails(exception.Message, StatusCodes.Status500InternalServerError),
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = details.StatusCode;
        await context.Response.WriteAsJsonAsync(details);
    }
}
