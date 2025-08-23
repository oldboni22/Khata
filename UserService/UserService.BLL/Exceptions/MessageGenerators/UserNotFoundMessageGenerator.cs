namespace UserService.BLL.Exceptions.MessageGenerators;

public static class UserNotFoundMessageGenerator
{
    public static string GenerateMessage(Guid userId) =>
        $"A user with id {userId} was not found.";

}