using UserService.DAL.Models.Enums;

namespace UserService.BLL.Utilities.MessageGenerators.Logs;

public static class RelationDoesNotExistLogMessageGenerator
{
    public static string GenerateMessage(Guid userId, Guid topicId, UserTopicRelationStatus status) =>
        $"User with id {userId} does not have a relation {status} to a topic with id {topicId}.";
}