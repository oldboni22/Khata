using Shared.Enums;
using UserService.BLL.Utilities.MessageGenerators.Exceptions;

namespace UserService.BLL.Exceptions.Relations;

public class RelationDoesNotExistException(Guid userId, Guid topicId, UserTopicRelationStatus status) : 
    Exception(RelationDoesNotExistExceptionMessageGenerator.GenerateMessage(userId, topicId, status))
{
}
