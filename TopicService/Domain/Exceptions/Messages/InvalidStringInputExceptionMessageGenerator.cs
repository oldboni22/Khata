namespace Domain.Exceptions.Messages;

public static class InvalidStringInputExceptionMessageGenerator
{
    public static string Generate(int minLength, int maxLength) =>
        $"Length must be greater than {maxLength} and lower than {maxLength}.";
}
