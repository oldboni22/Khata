using Shared.Exceptions.Messages;

namespace Shared.Exceptions;

public class ForbiddenException() : Exception(ForbiddenExceptionMessage.Message)
{
    
}
