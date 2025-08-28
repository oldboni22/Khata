using Microsoft.AspNetCore.Mvc.Filters;
using Shared;
using UserService.API.Exceptions;
using UserService.API.Utilities.ApiKeys;
using UserService.BLL.Services;

namespace UserService.API.ActionFilters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class ApiKeyFilter(ApiType apiType) : Attribute, IAsyncActionFilter 
{
    private const string HeaderPath = "X-API-KEY";
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        
        context.HttpContext.Request.Headers.TryGetValue(HeaderPath, out var headerKey);
        
        if(headerKey.ToString() != GetApiKey(configuration))
        {
            throw new UnauthorizedException();
        }
            
        await next();
    }

    private string GetApiKey(IConfiguration configuration)
    {
        var path = apiType switch
        {
            ApiType.Auth0 => ConfigurationKeys.Auth0ApiKey,
            _ => throw new ArgumentException(),
        };
        
        return configuration[path]!;
    }
}
