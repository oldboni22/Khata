using Domain.Entities;

namespace Domain.Exceptions.Messages;

public static class EntityNotFoundExceptionMessage<T> where T : EntityBase
{
    public static string Generate(Guid id) => $"Entity {typeof(T).Name} with id {id} was not found.";
}
