namespace UserService.BLL.Utilities.MessageGenerators.Exceptions;

public static class UserBannedExceptionMessageGenerator
{
    public static string GenerateMessage(Guid userId, Guid topicId) =>
        $"User with id {userId} is banned from topic with id {topicId}.";
}
