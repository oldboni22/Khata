/*
using Infrastructure.gRpc;

namespace UserService.BLL.gRpc;

public interface ITopicGRpcClient
{
    Task<bool> IsOwnerAsync(Guid userId, Guid topicId);
}

public class TopicGRpcClientWrapper(TopicGRpcApi.TopicGRpcApiClient client) : ITopicGRpcClient
{
    public async Task<bool> IsOwnerAsync(Guid userId, Guid topicId)
    {
        var request = new TopicUserRequest
        {
            UserId = userId.ToString(),
            TopicId = topicId.ToString()
        };
        
        var response = await client.IsOwnerAsync(request);
        
        return response.Result;
    }
}
*/
