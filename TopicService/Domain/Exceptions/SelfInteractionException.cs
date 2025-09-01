using Domain.Exceptions.Messages;

namespace Domain.Exceptions;

public class SelfInteractionException() : Exception(SelfInteractionExceptionMessage.Message)
{
    
}
