namespace UserService.BLL.Utilities.MessageGenerators.Exceptions;

public static class ForbiddenExceptionMessageGenerator
{
    public static string GenerateMessage(Guid id) =>
        $"User with ID '{id}' does not have permission to perform this action.";
}
