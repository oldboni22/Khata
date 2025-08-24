using UserService.DAL.Models.Enums;

namespace UserService.BLL.Utilities.MessageGenerators.Exceptions;

public static class RelationDoesNotExistExceptionMessageGenerator
{
    public static string GenerateMessage(Guid userId, Guid topicId, UserTopicRelationStatus status) =>
        $"A relation with status {status} between user with id {userId} and topic with id {topicId} was not found.";
}
