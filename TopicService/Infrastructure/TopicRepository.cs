using Domain.Entities;
using Domain.RepositoryContracts;

namespace Infrastructure;

public class TopicRepository(TopicServiceContext context) : GenericRepository<Topic>(context), ITopicRepository
{
}
