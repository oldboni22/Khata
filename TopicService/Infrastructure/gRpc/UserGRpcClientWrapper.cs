namespace Infrastructure.gRpc;

public interface IUserGRpcClient
{
    Task<List<Guid>> FindBannedTopicsIdAsync(Guid userId);
    
    Task<bool> IsModeratorAsync(Guid userId, Guid topicId);

    Task<Guid> FindUserIdByAuth0IdAsync(string auth0Id);
}

public class UserGRpcClientWrapper(UserGRpcApi.UserGRpcApiClient client) : IUserGRpcClient 
{
    public async Task<List<Guid>> FindBannedTopicsIdAsync(Guid userId)
    {
        var request = new BannedUserTopicsRequest{ UserId = userId.ToString() };

        var response = await client.FindBannedTopicsIdAsync(request);
        
        return response.TopicIds.Select(Guid.Parse).ToList();
    }

    public async Task<bool> IsModeratorAsync(Guid userId, Guid topicId)
    {
        var  request = new TopicUserStatusRequest
        {
            UserId = userId.ToString(),
            TopicId = topicId.ToString()
        };
        
        var response = await client.IsModeratorAsync(request);

        return response.Result;
    }

    public async Task<Guid> FindUserIdByAuth0IdAsync(string auth0Id)
    {
        var request = new UserIdByAuth0IdRequest { UserAuth0Id = auth0Id };

        var response = await client.FindUserIdByAuth0IdAsync(request);

        return Guid.Parse(response.UserId);
    }
}
