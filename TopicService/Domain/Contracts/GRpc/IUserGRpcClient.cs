using Shared.Enums;

namespace Domain.Contracts.GRpc;

public interface IUserGRpcClient
{
    Task<List<Guid>> FindBannedTopicsIdAsync(Guid userId);

    Task<bool> HasStatusAsync(Guid userId, Guid topicId, UserTopicRelationStatus status);
    
    Task<Guid> FindUserIdByAuth0IdAsync(string auth0Id);
}
