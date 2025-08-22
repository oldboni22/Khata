using UserService.DAL.Models.Enums;

namespace UserService.BLL.Exceptions.Relations;

public class RelationAlreadyExistsException(Guid userId, Guid topicId, UserTopicRelationStatus status) : 
    Exception($"User with id {userId} already has a relation with status {status} to topic with id {topicId}.")
{
    
}