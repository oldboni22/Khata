namespace UserService.BLL.Utilities.MessageGenerators.Logs;

public static class ForbiddenLogMessageGenerator
{
    public static string GenerateMessage(Guid id) =>
        $"Access denied for user with ID '{id}'. User does not have the necessary permissions to perform this action.";
}
