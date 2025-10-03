using Domain.Exceptions.Messages;

namespace Domain.Exceptions;

public class MediaNotFoundException(Guid subjectId) 
    : NotFoundException(MediaNotFoundExceptionMessageGenerator.Generate(subjectId))
{
    
}
