using UserService.DAL.Models.Enums;

namespace UserService.BLL.Utilities.MessageGenerators.Logs;

public static class UserBannedLogMessageGenerator
{
    public static string GenerateMessage(Guid userId, Guid topicId) =>
        $"User with id {userId} is banned from a topic with id {topicId}.";
}
