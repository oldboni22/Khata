using Domain.Entities;
using Domain.RepositoryContracts;

namespace Infrastructure.Repositories;

public class TopicRepository(TopicServiceContext context) : GenericRepository<TopicReadDto>(context), ITopicRepository
{
}
