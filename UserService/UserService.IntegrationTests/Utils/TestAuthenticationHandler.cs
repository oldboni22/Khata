using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UserService.IntegrationTests.Utils;

public class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISystemClock clock)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, clock)
{
    public const string SchemeName = "Test";
    
    public const string ReturnIdHeader = "test-user-id";
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(SchemeName);

        if (!Context.Request.Headers.TryGetValue(ReturnIdHeader, out var headerSection))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        
        var auth0Id = headerSection.FirstOrDefault();
        
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, auth0Id!));  
        
        var claims = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(claims, SchemeName);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
