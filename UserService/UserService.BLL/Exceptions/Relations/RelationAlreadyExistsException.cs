using System.Text;
using UserService.BLL.Exceptions.MessageGenerators;
using UserService.DAL.Models.Enums;

namespace UserService.BLL.Exceptions.Relations;

public class RelationAlreadyExistsException(Guid userId, Guid topicId, UserTopicRelationStatus status) : 
    Exception(RelationAlreadyExistsMessageGenerator.GenerateMessage(userId, topicId, status))
{
}
