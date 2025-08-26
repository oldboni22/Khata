using UserService.DAL.Models.Entities;

namespace UserService.DAL.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
}

public class UserRepository(UserServiceContext context) : GenericRepository<User>(context), IUserRepository 
{
}
