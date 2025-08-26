using Microsoft.EntityFrameworkCore;
using UserService.DAL.Models.Entities;

namespace UserService.DAL.Repositories;

public interface IUserTopicRelationRepository : IGenericRepository<UserTopicRelation>
{
}

public class UserTopicRelationRepository(UserServiceContext context) : 
    GenericRepository<UserTopicRelation>(context), IUserTopicRelationRepository
{
}
