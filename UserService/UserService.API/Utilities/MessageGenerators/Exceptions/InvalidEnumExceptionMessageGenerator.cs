namespace UserService.API.Utilities.MessageGenerators.Exceptions;

public static class InvalidEnumExceptionMessageGenerator<T>
{
    public static string GenerateMessage() =>
        $"Invalid enum string for {typeof(T).Name} was given.";
}
