using System;
using System.Security.Claims;

namespace Shared.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? GetSenderUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            
            return userIdClaim is null ? null : Guid.Parse(userIdClaim.Value);
        }
    }
}
