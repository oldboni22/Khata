using System;
using System.Security.Claims;

namespace Shared.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool TryGetSenderUserId(this ClaimsPrincipal principal, out Guid senderId)
        {
            var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            senderId = Guid.Empty;
            
            return !string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out senderId);
        }
    }
}