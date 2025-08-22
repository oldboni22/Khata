using UserService.BLL.Exceptions.MessageGenerators;

namespace UserService.BLL.Exceptions.Relations;

public class UserBannedException(Guid userId, Guid topicId) : 
    Exception(UserBannedMessageGenerator.GenerateMessage(userId, topicId))
{
}