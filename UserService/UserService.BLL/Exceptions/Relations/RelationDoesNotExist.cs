using UserService.DAL.Models.Enums;

namespace UserService.BLL.Exceptions.Relations;

public class RelationDoesNotExist(Guid userId, Guid topicId, UserTopicRelationStatus status) : 
    Exception($"User with id {userId} does not have a relation with status {status} to topic with id {topicId}.")
{
}