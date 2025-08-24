using UserService.BLL.Utilities.MessageGenerators.Exceptions;

namespace UserService.BLL.Exceptions.Relations;

public class UserBannedException(Guid userId, Guid topicId) : 
    Exception(UserBannedExceptionMessageGenerator.GenerateMessage(userId, topicId))
{
}
