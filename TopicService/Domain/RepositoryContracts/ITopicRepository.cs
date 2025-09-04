using System.Linq.Expressions;
using Domain.Entities;
using Shared.Filters;
using Shared.PagedList;

namespace Domain.RepositoryContracts;

public interface ITopicRepository : IGenericRepository<TopicReadDto>
{
}
