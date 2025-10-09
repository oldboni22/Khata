using System.ComponentModel.DataAnnotations;
using UserService.API.Exceptions;
using UserService.API.Utilities;
using UserService.BLL.Exceptions;
using UserService.BLL.Exceptions.Relations;

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
        if(exception.StackTrace is not null)
        {
            logger.Error(exception.StackTrace);
        }
        
        logger.Error(exception.Message);

        ExceptionDetails details = exception switch
        {
            UnauthorizedException => new ExceptionDetails(exception.Message, StatusCodes.Status401Unauthorized),
            
            ForbiddenException or UserBannedException => new ExceptionDetails(exception.Message, StatusCodes.Status403Forbidden),
            
            ValidationException or BadRequestException or BadHttpRequestException or ArgumentException 
                => new ExceptionDetails(exception.Message, StatusCodes.Status400BadRequest),
            
            NotFoundException =>  new(exception.Message, StatusCodes.Status404NotFound),
            
            _ => new(exception.Message, StatusCodes.Status500InternalServerError),
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = details.StatusCode;
        await context.Response.WriteAsJsonAsync(details);
    }
}
