using Shared.Enums;

namespace UserService.BLL.Utilities.MessageGenerators.Logs;

public static class RemoveBannedUserStatusLogMessageGenerator
{
    public static string GenerateMessage(Guid userId, Guid topicId, UserTopicRelationStatus status) =>
        $"User with id {userId} is being baned from topic with id {topicId}, but has a relation {status}. Removing that relation.";
}
