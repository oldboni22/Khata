namespace UserService.BLL.Exceptions.MessageGenerators;

public static class UserBannedMessageGenerator
{
    public static string GenerateMessage(Guid userId, Guid topicId) =>
        $"User with id {userId} is banned from topic with id {topicId}.";
}