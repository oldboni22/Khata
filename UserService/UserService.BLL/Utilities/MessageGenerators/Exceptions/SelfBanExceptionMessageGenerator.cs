namespace UserService.BLL.Utilities.MessageGenerators.Exceptions;

public static class SelfBanExceptionMessageGenerator
{
    public static string GenerateMessage() =>
        $"Cannot ban yourself.";
}