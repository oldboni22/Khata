using UserService.API.Utilities.MessageGenerators.Exceptions;
using UserService.BLL.Utilities.MessageGenerators.Exceptions;

namespace UserService.API.Exceptions.Unauthorized;

public class UnauthorizedUserException() : UnauthorizedException(UnauthorizedUserExceptionMessage.Message)
{
}
