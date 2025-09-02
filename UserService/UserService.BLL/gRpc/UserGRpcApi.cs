using Grpc.Core;
using Infrastructure.gRpc;
using Shared.Enums;
using UserService.BLL.Services;

namespace UserService.BLL.gRpc;

public class UserGRpcApi(IUserService userService) : Infrastructure.gRpc.UserGRpcApi.UserGRpcApiBase 
{
    public override async Task<TopicUserResponse> IsModerator(TopicUserRequest request, ServerCallContext context)
    {
        var userId = Guid.Parse(request.UserId);
        var topicId = Guid.Parse(request.TopicId);
        
        var result = await userService.DoesUserHaveTopicStatusAsync(userId, topicId, UserTopicRelationStatus.Moderator);

        return new TopicUserResponse { Result = result };
    }

    public override async Task<BannedUserTopicsResponse> GetBannedTopicsId(BannedUserTopicsRequest request, ServerCallContext context)
    {
        var userId = Guid.Parse(request.UserId);

        var result = await userService.FindBannedTopicsIds(userId);
        var idList = result.Select(id => id.ToString());
        
        var response = new BannedUserTopicsResponse();
        response.TopicIds.AddRange(idList);
        
        return response;
    }
    
}
