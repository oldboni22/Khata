using Domain.Entities;
using Domain.Exceptions.Messages;

namespace Domain.Exceptions;

public class EntityNotFoundException<T>(Guid id) : NotFoundException(EntityNotFoundExceptionMessage<T>.Generate(id)) 
    where T : EntityBase 
{
    
}
