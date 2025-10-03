namespace UserService.API.Exceptions;

public class InvalidConfigurationKeyException() : Exception(ExceptionMessage)
{
    private const string ExceptionMessage = "Invalid configuration key";
}
