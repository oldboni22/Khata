using UserService.BLL.Utilities.MessageGenerators.Exceptions;
using UserService.DAL.Models.Enums;

namespace UserService.BLL.Exceptions.Relations;

public class RelationDoesNotExistException(Guid userId, Guid topicId, UserTopicRelationStatus status) : 
    Exception(RelationDoesNotExistExceptionMessageGenerator.GenerateMessage(userId, topicId, status))
{
}
