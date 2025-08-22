namespace UserService.BLL.Exceptions.Relations;

public class UserBannedException(Guid userId, Guid topicId) : 
    Exception(ExceptionMessages.GenerateUserBannedMessage(userId, topicId))
{
    
}