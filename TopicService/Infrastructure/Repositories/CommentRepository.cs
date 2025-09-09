using Domain.Contracts.RepositoryContracts;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class CommentRepository(TopicServiceContext context) : GenericReadRepository<Comment>(context), ICommentRepository
{
}
