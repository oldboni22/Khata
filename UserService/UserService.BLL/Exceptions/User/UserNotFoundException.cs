using UserService.BLL.Exceptions.MessageGenerators;

namespace UserService.BLL.Exceptions.User;

public class UserNotFoundException(Guid id) : NotFoundException(UserNotFoundMessageGenerator.GenerateMessage(id))
{
}
