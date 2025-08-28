using System;
using System.Security.Claims;

namespace Shared.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GeAuth0Id(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            
            return userIdClaim?.Value;
        }
    }
}
