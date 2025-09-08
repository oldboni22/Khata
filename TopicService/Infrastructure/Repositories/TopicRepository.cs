using Domain.Contracts.RepositoryContracts;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class TopicRepository(TopicServiceContext context) : GenericRepository<Topic>(context), ITopicRepository
{
}
