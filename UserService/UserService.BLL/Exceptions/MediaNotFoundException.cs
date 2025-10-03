using UserService.BLL.Utilities.MessageGenerators.Exceptions;

namespace UserService.BLL.Exceptions;

public class MediaNotFoundException(Guid subjectId) 
    : NotFoundException(MediaNotFoundExceptionMessageGenerator.Generate(subjectId))
{
    
}
