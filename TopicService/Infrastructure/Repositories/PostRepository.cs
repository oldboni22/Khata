using Domain.Contracts.RepositoryContracts;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class PostRepository(TopicServiceContext context) : GenericRepository<Post>(context), IPostRepository
{
}
