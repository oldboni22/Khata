using Domain.Entities;

namespace Domain.Contracts.RepositoryContracts;

public interface ITopicRepository : IGenericRepository<Topic>
{
    Task<bool> IsOwnerAsync(Guid topicId, Guid userId, CancellationToken cancellationToken = default);
}
