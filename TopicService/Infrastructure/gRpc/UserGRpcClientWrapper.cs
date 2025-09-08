using Domain.Contracts.GRpc;
using Shared.Enums;

namespace Infrastructure.gRpc;

public class UserGRpcClientWrapper(UserGRpcApi.UserGRpcApiClient client) : IUserGRpcClient 
{
    public async Task<List<Guid>> FindBannedTopicsIdAsync(Guid userId)
    {
        var request = new BannedUserTopicsRequest{ UserId = userId.ToString() };

        var response = await client.FindBannedTopicsIdAsync(request);
        
        return response.TopicIds.Select(Guid.Parse).ToList();
    }

    public async Task<bool> HasStatusAsync(Guid userId, Guid topicId, UserTopicRelationStatus status)
    {
        var  request = new TopicUserStatusRequest
        {
            UserId = userId.ToString(),
            TopicId = topicId.ToString(),
            
            Status = (int)status
        };
        
        var response = await client.HasStatusAsync(request);

        return response.Result;
    }

    public async Task<Guid> FindUserIdByAuth0IdAsync(string auth0Id)
    {
        var request = new UserIdByAuth0IdRequest { UserAuth0Id = auth0Id };

        var response = await client.FindUserIdByAuth0IdAsync(request);

        return Guid.Parse(response.UserId);
    }
}
