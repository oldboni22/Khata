using Domain.Entities;

namespace Domain.RepositoryContracts;

public interface ITopicRepository : IGenericRepository<Topic>
{
    Task<bool> DoesUserOwnTopic(Guid topicId, Guid userId, CancellationToken cancellationToken = default);
}
