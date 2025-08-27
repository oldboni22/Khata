namespace UserService.API.Exceptions.Unauthorized;

public class UnauthorizedException(string message) : Exception(message)
{
}
