namespace Domain.Exceptions.Messages;

public static class MediaNotFoundExceptionMessageGenerator
{
    public static string Generate(Guid id) =>
        $"Media related to subject with id {id} was not found!";
}
