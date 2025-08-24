using System.Text;
using UserService.BLL.Utilities.MessageGenerators.Exceptions;
using UserService.DAL.Models.Enums;

namespace UserService.BLL.Exceptions.Relations;

public class RelationAlreadyExistsException(Guid userId, Guid topicId, UserTopicRelationStatus status) : 
    Exception(RelationAlreadyExistsExceptionMessageGenerator.GenerateMessage(userId, topicId, status))
{
}
