namespace UserService.BLL.Exceptions;

public class UserBannedException(Guid userId, Guid topicId) : Exception($"User with id {userId} is banned from topic with id {topicId}.")
{
    
}