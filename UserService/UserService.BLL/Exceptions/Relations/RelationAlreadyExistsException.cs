using System.Text;
using Shared.Enums;
using UserService.BLL.Utilities.MessageGenerators.Exceptions;

namespace UserService.BLL.Exceptions.Relations;

public class RelationAlreadyExistsException(Guid userId, Guid topicId, UserTopicRelationStatus status) : 
    Exception(RelationAlreadyExistsExceptionMessageGenerator.GenerateMessage(userId, topicId, status))
{
}
