namespace UserService.BLL.Utilities.MessageGenerators.Logs;

public static class EntityNotFoundLogMessageGenerator<TEntity>
{
    public static string GenerateMessage(Guid id) =>
        $"An entity of type {typeof(TEntity).Name} with id {id} was not found.";
}
