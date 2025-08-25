namespace UserService.API.Utilities.MessageGenerators.Exceptions;

public static class InvalidRelationStringLiteralExceptionMessageGenerator
{
    public static string GenerateMessage(string literal) =>
        $"Invalid string literal '{literal}' for UserTopicRelationStatus enum.";
}