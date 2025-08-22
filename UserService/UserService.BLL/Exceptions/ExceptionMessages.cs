using UserService.DAL.Models.Enums;

namespace UserService.BLL.Exceptions;

public class ExceptionMessages
{
    public static string GenerateRelationAlreadyExistsMessage(Guid userId, Guid topicId, UserTopicRelationStatus status) =>
        $"A relation with status {status} between user with id {userId} and topic with id {topicId} already exists.";

    public static string GenerateRelationDoesNotExistMessage(Guid userId, Guid topicId, UserTopicRelationStatus status) =>
        $"A relation with status {status} between user with id {userId} and topic with id {topicId} was not found.";
    
    public static string GenerateUserBannedMessage(Guid userId, Guid topicId) =>
        $"User with id {userId} is banned from topic with id {topicId}.";
}