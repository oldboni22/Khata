using Infrastructure.gRpc;
using NotificationService.Domain.Contracts;

namespace NotificationService.Infrastructure.GRpc;

public class UserGRpcClient(UserGRpcApi.UserGRpcApiClient client) : IUserGrpcService
{
    public async Task<Guid?> GetUserIdAsync(string auth0Id)
    {
        var request = new UserIdByAuth0IdRequest { UserAuth0Id = auth0Id };

        var response = await client.FindUserIdByAuth0IdAsync(request);

        return Guid.Parse(response.UserId);
    }
}