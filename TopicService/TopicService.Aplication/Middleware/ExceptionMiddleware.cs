using Shared.Exceptions;
using UserService.API.Utilities;

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

        ExceptionDetails details = new ExceptionDetails(exception.Message,1);
            
        details = exception switch
        {
            ForbiddenException => details with {StatusCode = StatusCodes.Status403Forbidden},
            _ => details with {StatusCode = StatusCodes.Status500InternalServerError},
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = details.StatusCode;
        await context.Response.WriteAsJsonAsync(details);
    }
}
