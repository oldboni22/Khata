namespace UserService.BLL.Exceptions;

public class UserNotFoundException(Guid id) : NotFoundException($"A user with id {id} was not found.")
{
    
}