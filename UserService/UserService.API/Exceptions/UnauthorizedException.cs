using UserService.API.Utilities.MessageGenerators.Exceptions;

namespace UserService.API.Exceptions;

public class UnauthorizedException() : Exception(UnauthorizedExceptionMessage.Message)
{
}
