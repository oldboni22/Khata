namespace Shared.Exceptions;

public class NoClaimPrincipalException() : Exception(ExceptionMessage)
{
    private const string ExceptionMessage = "Failed to authenticate user.";
}
