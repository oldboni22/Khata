using Microsoft.AspNetCore.Mvc.Filters;
using UserService.API.Exceptions.Unauthorized;
using UserService.API.Utilities.ApiKeys;

namespace UserService.API.ActionFilters;

public class ApiKeyFilter(IConfiguration configuration, ApiType apiType) : IAsyncActionFilter 
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
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
            ApiType.Auth0 => (LocalApiKeyPaths.Auth0),
            _ => throw new ArgumentException(),
        };
    }
}
