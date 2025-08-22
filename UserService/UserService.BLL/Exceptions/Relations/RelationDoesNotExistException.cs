using UserService.BLL.Exceptions.MessageGenerators;
using UserService.DAL.Models.Enums;

namespace UserService.BLL.Exceptions.Relations;

public class RelationDoesNotExistException(Guid userId, Guid topicId, UserTopicRelationStatus status) : 
    Exception(RelationDoesNotExistMessageGenerator.GenerateMessage(userId, topicId, status))
{
}