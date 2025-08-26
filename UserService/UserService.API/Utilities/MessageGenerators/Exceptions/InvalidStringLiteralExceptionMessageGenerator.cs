namespace UserService.API.Utilities.MessageGenerators.Exceptions;

public static class InvalidStringLiteralExceptionMessageGenerator
{
    public static string GenerateMessage(string statusLiteral) =>
        $"The provided string literal '{statusLiteral}' is invalid. Please provide a valid string literal.";
}