using UserService.DAL.Models.Enums;

namespace UserService.BLL.Exceptions.Relations;

public class RelationDoesNotExist(Guid userId, Guid topicId, UserTopicRelationStatus status) : 
    Exception(ExceptionMessages.GenerateRelationDoesNotExistMessage(userId, topicId, status))
{
}