using Domain.RepositoryContracts;
using Grpc.Core;

namespace Infrastructure.gRpc;

public class GRpcService(ITopicRepository topicRepository) : TopicGRpcApi.TopicGRpcApiBase
{
    /*public override async Task<TopicUserResponse> IsOwner(TopicUserRequest request, ServerCallContext context)
    {
        var userId = Guid.Parse(request.UserId);
        var topicId = Guid.Parse(request.TopicId);

        var result = await topicRepository.DoesUserOwnTopic(topicId, userId, CancellationToken.None);

        return new TopicUserResponse { Result = result };
    }*/
}
