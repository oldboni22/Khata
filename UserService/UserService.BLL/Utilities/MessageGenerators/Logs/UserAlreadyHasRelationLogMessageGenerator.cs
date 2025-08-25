using Shared.Enums;

namespace UserService.BLL.Utilities.MessageGenerators.Logs;

public static class UserAlreadyHasRelationLogMessageGenerator
{
    public static string GenerateMessage(Guid userId, Guid topicId, UserTopicRelationStatus status) =>
        $"A user with id {userId} already has a relation {status} to topic with id {topicId}.";
}
