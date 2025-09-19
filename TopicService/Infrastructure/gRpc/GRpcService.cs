using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Grpc.Core;

namespace Infrastructure.gRpc;

public class GRpcService(ITopicRepository topicRepository) : TopicGRpcApi.TopicGRpcApiBase
{
    public override async Task<TopicUserResponse> IsOwner(TopicUserStatusRequest request, ServerCallContext context)
    {
        var userId = Guid.Parse(request.UserId);
        var topicId = Guid.Parse(request.TopicId);

        var result = await topicRepository.IsOwnerAsync(topicId, userId);

        return new TopicUserResponse { Result = result};
    }
}
