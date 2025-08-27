using Microsoft.AspNetCore.Mvc.Filters;
using UserService.API.Exceptions.Unauthorized;
using UserService.API.Utilities.ApiKeys;

namespace UserService.API.ActionFilters;

public class ApiKeyFilter(ApiType apiType) : Attribute, IAsyncActionFilter 
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        
        var localApiKeyPath = GetApiKeyPaths();
        
        context.HttpContext.Request.Headers.TryGetValue(HeaderApiKeyPath.Path, out var headerKey);
        
        if(headerKey.ToString() != configuration[localApiKeyPath])
        {
            throw new UnauthorizedWebhookException(context.HttpContext.Request.Path);
        }
            
        await next();
    }

    private string GetApiKeyPaths()
    {
        return apiType switch
        {
            ApiType.Auth0 => (LocalApiKeyPath.Auth0),
            _ => throw new ArgumentException(),
        };
    }
}
