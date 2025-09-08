namespace TopicService.API.Utilities.LogMessages;

public static class EntityNotFoundLogMessage<T> 
{
    public static string Generate(Guid id) => $"Entity {nameof(T)} with id {id} was not found.";
}
