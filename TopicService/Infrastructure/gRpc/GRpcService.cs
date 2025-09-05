using Domain.Contracts.RepositoryContracts;
using Grpc.Core;

namespace Infrastructure.gRpc;

public class GRpcService(ITopicRepository topicRepository) : TopicGRpcApi.TopicGRpcApiBase
{
    public override async Task<TopicUserResponse> IsOwner(TopicUserStatusRequest request, ServerCallContext context)
    {
        var userId = Guid.Parse(request.UserId);
        var topicId = Guid.Parse(request.TopicId);

        var topic = await topicRepository.FindByIdAsync(topicId);

        if (topic is null)
        {
            return new TopicUserResponse { Result = false };
        }

        return new TopicUserResponse { Result = topic.OwnerId == userId };
    }
}
