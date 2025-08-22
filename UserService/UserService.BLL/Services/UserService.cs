using AutoMapper;
using Serilog;
using UserService.BLL.Exceptions;
using UserService.BLL.Models.User;
using UserService.DAL.Models.Entities;
using UserService.DAL.Repositories;

namespace UserService.BLL.Services;

public interface IUserService : IGenericService<User, UserModel, UserCreateModel, UserUpdateModel>
{
    Task SubscribeUser(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
}

public class UserService(IGenericRepository<User> repository, UserTopicRelationRepository userTopicRelationRepository, IMapper mapper, ILogger logger) : 
    GenericService<User, UserModel, UserCreateModel, UserUpdateModel>(repository, mapper, logger), IUserService
{
    public async Task SubscribeUser(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        if (! await Repository.ExistsAsync(userId, cancellationToken))
        {
            Logger.Warning($"A user with id {userId} was not found.");

            throw new UserNotFoundException(userId);
        }

        if (await userTopicRelationRepository.ExistsAsync(userId, topicId, cancellationToken))
        {
            Logger.Warning($"A user with id {userId} was already subscribed to a topic with id {topicId}.");
        }
        
    }
}