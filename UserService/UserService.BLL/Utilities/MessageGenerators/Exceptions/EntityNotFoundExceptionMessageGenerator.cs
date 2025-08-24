namespace UserService.BLL.Utilities.MessageGenerators.Exceptions;

public static class EntityNotFoundExceptionMessageGenerator<TEntity>
{
    public static string GenerateMessage(Guid id) =>
        $"An entity {typeof(TEntity).Name} with id {id} was not found.";
}
