using System.Text;
using UserService.DAL.Models.Enums;

namespace UserService.BLL.Exceptions.Relations;

public class RelationAlreadyExistsException(Guid userId, Guid topicId, UserTopicRelationStatus status) : 
    Exception(ExceptionMessages.GenerateRelationAlreadyExistsMessage(userId, topicId, status))
{
}
