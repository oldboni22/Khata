using UserService.BLL.Utilities.MessageGenerators.Exceptions;

namespace UserService.BLL.Exceptions;

public class EntityNotFoundException<TEntity>(Guid id) :
    NotFoundException(EntityNotFoundExceptionMessageGenerator<TEntity>.GenerateMessage(id))
{
}
