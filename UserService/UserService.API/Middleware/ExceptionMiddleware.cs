using UserService.API.Utilities;

namespace UserService.API.Middleware;

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
        var path = context.Request.Path;
        logger.Error(exception, $"An error occurred while processing the request at {path}.");

        var response = exception switch
        {
            _ => StatusCodes.Status500InternalServerError,
        };
        
        var details = new ExceptionDetails(exception.Message, response);
        
        await context.Response.WriteAsync(details.ToString());
    }
    
}