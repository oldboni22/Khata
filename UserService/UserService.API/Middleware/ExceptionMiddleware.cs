using System.ComponentModel.DataAnnotations;
using UserService.API.Utilities;
using UserService.BLL.Exceptions;

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
        logger.Error(exception.Message);

        ExceptionDetails details = exception switch
        {
            ValidationException => new ExceptionDetails(exception.Message, StatusCodes.Status400BadRequest),
            NotFoundException =>  new(exception.Message, StatusCodes.Status404NotFound),
            _ => new(exception.Message, StatusCodes.Status500InternalServerError),
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = details.StatusCode;
        await context.Response.WriteAsJsonAsync(details);
    }
}
