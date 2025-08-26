using UserService.BLL.Utilities.MessageGenerators.Exceptions;

namespace UserService.BLL.Exceptions;

public class ForbiddenException(Guid id) 
    : Exception(ForbiddenExceptionMessageGenerator.GenerateMessage(id))
{
}
