namespace Infrastructure.gRpc;

public interface IUserGRpcClient
{
    Task<List<Guid>> GetBannedTopicsId(Guid userId);
    
    Task<bool> IsModeratorAsync(Guid userId, Guid topicId);
}

public class UserGRpcClientWrapper(UserGRpcApi.UserGRpcApiClient client) : IUserGRpcClient 
{
    public async Task<List<Guid>> GetBannedTopicsId(Guid userId)
    {
        var request = new BannedUserTopicsRequest{ UserId = userId.ToString() };

        var response = await client.GetBannedTopicsIdAsync(request);
        
        return response.TopicIds.Select(Guid.Parse).ToList();
    }

    public async Task<bool> IsModeratorAsync(Guid userId, Guid topicId)
    {
        var  request = new TopicUserRequest
        {
            UserId = userId.ToString(),
            TopicId = topicId.ToString()
        };
        
        var response = await client.IsModeratorAsync(request);

        return response.Result;
    }
}
