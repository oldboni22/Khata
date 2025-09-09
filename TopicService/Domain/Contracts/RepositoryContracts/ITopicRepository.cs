using Domain.Entities;

namespace Domain.Contracts.RepositoryContracts;

public interface ITopicRepository : IGenericWriteRepository<Topic>, IGenericReadRepository<Topic>
{
}
