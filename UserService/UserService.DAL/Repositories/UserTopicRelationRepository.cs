using Microsoft.EntityFrameworkCore;
using UserService.DAL.Models.Entities;

namespace UserService.DAL.Repositories;

public interface IUserTopicRelationRepository : IGenericRepository<UserTopicRelation>
{
    Task<bool> ExistsAsync(Guid userId, Guid topicId, CancellationToken cancellationToken);
}

public class UserTopicRelationRepository(UserServiceContext context) : 
    GenericRepository<UserTopicRelation>(context), IUserTopicRelationRepository
{
    public async Task<bool> ExistsAsync(Guid userId, Guid topicId, CancellationToken cancellationToken)
    {
        return await Context
            .Set<UserTopicRelation>()
            .AnyAsync(utr => utr.UserId == userId && utr.TopicId == topicId, cancellationToken: cancellationToken);
    }
}