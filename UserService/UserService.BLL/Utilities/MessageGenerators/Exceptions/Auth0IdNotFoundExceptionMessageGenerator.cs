namespace UserService.BLL.Utilities.MessageGenerators.Exceptions;

public static class Auth0IdNotFoundExceptionMessageGenerator
{
    public static string GenerateMessage(string auth0Id) => 
        $"User with auth0 id {auth0Id} was not found.";
}
