using Grpc.Core;
using Infrastructure.gRpc;
using Shared.Enums;
using UserService.BLL.Services;

namespace UserService.BLL.gRpc;

public class UserGRpcApi(IUserService userService) : Infrastructure.gRpc.UserGRpcApi.UserGRpcApiBase 
{
    public override async Task<TopicUserResponse> HasStatus(
        TopicUserStatusRequest request, ServerCallContext context)
    {
        var userId = Guid.Parse(request.UserId);
        var topicId = Guid.Parse(request.TopicId);
        var status = (UserTopicRelationStatus)request.Status;
        
        var result = await userService
            .DoesUserHaveTopicStatusAsync(userId, topicId, status);

        return new TopicUserResponse { Result = result };
    }
    
    public override async Task<BannedUserTopicsResponse> FindBannedTopicsId(
        BannedUserTopicsRequest request, ServerCallContext context)
    {
        var userId = Guid.Parse(request.UserId);

        var result = await userService.FindBannedTopicsIdsAsync(userId);
        var idList = result.Select(id => id.ToString());
        
        var response = new BannedUserTopicsResponse();
        response.TopicIds.AddRange(idList);
        
        return response;
    }

    public override async Task<UserIdByAuth0IdResponse> FindUserIdByAuth0Id(
        UserIdByAuth0IdRequest request, ServerCallContext context)
    {
        var userId = await userService.FindUserIdByAuth0IdAsync(request.UserAuth0Id);

        return new UserIdByAuth0IdResponse { UserId = userId.ToString() };
    }

    public override async Task<UsersWithStatusResponse> FindUsersWithStatus(UsersWithStatusRequest request, ServerCallContext context)
    {
        var topicId = Guid.Parse(request.TopicId);
        var status = (UserTopicRelationStatus)request.Status;
        
        var userIds = await userService.FindUsersWithStatusAsync(topicId, status);

        var response = new UsersWithStatusResponse
        {
            UserIds = { userIds },
        };
        
        return response;
    }
}
