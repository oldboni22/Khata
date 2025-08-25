using AutoMapper;
using Serilog;
using Shared.Enums;
using UserService.BLL.Exceptions;
using UserService.BLL.Exceptions.Relations;
using UserService.BLL.Models;
using UserService.BLL.Models.User;
using UserService.BLL.Utilities.MessageGenerators.Logs;
using UserService.DAL.Models.Entities;
using UserService.DAL.Repositories;

namespace UserService.BLL.Services;

public interface IUserService : IGenericService<User, UserModel, UserCreateModel, UserUpdateModel>
{
    Task SubscribeUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task UnsubscribeUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task BanUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task UnbanUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task PromoteUserToModeratorAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task DemoteUserFromModeratorAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
}

public class UserService(IGenericRepository<User> userRepository, IUserTopicRelationRepository userTopicRelationRepository,
    IMapper mapper, ILogger logger) : 
    GenericService<User, UserModel, UserCreateModel, UserUpdateModel>(userRepository, mapper, logger), IUserService
{
    public async Task SubscribeUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(relation => relation.UserId == userId && relation.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);
        
        if (DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Subscribed, out var subscriptionRelationId))
        {
            Logger.Warning(UserAlreadyHasRelationLogMessageGenerator
                .GenerateMessage(userId, topicId, UserTopicRelationStatus.Subscribed));
            
            throw new RelationAlreadyExistsException(userId, topicId, UserTopicRelationStatus.Subscribed);
        }
        
        if (DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Banned, out var banRelationId))
        {
            Logger.Warning(UserBannedLogMessageGenerator.GenerateMessage(userId, topicId));

            throw new UserBannedException(userId, topicId);
        }

        var relation = new UserTopicRelationModel()
        {
            UserId = userId,
            TopicId = topicId,
            TopicRelationStatus = UserTopicRelationStatus.Subscribed
        };

        var relationEntity = Mapper.Map<UserTopicRelation>(relation);
        
        await userTopicRelationRepository.CreateAsync(relationEntity, cancellationToken);
    }

    public async Task UnsubscribeUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);
        
        if (!DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Subscribed, out var subscriptionRelationId))
        {
            Logger.Warning(RelationDoesNotExistLogMessageGenerator
                .GenerateMessage(userId, topicId, UserTopicRelationStatus.Subscribed));
            
            throw new RelationDoesNotExistException(userId, topicId, UserTopicRelationStatus.Subscribed);
        }
        
        if (DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Banned, out var banRelationId))
        {
            Logger.Warning(UserBannedLogMessageGenerator.GenerateMessage(userId, topicId));
            
            throw new UserBannedException(userId, topicId);
        }
        
        var relationModel = relationModels.First(relation => relation.TopicRelationStatus == UserTopicRelationStatus.Subscribed);
        
        await userTopicRelationRepository.DeleteAsync(relationModel.Id, cancellationToken);
    }

    public async Task BanUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);

        if (DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Banned, out var banRelationId))
        {
            Logger.Warning(UserAlreadyHasRelationLogMessageGenerator
                .GenerateMessage(userId, topicId, UserTopicRelationStatus.Banned));
            
            throw new RelationAlreadyExistsException(userId, topicId, UserTopicRelationStatus.Banned);
        }
        
        if (DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Subscribed, out var subscriptionRelationId))
        {
            Logger.Information(RemoveBannedUserStatusLogMessageGenerator
                .GenerateMessage(userId, topicId, UserTopicRelationStatus.Subscribed));
            
            await userTopicRelationRepository.DeleteAsync(subscriptionRelationId, cancellationToken);
        }

        if (DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Moderator, out var moderationRelationId))
        {
            Logger.Information(RemoveBannedUserStatusLogMessageGenerator
                .GenerateMessage(userId, topicId, UserTopicRelationStatus.Moderator));
            
            await userTopicRelationRepository.DeleteAsync(moderationRelationId, cancellationToken);
        }
        
        var relationModel = new UserTopicRelationModel()
        {
            UserId = userId,
            TopicId = topicId,
            TopicRelationStatus = UserTopicRelationStatus.Banned
        };
        
        var relationEntity = Mapper.Map<UserTopicRelation>(relationModel);
        
        await userTopicRelationRepository.CreateAsync(relationEntity, cancellationToken);
    }

    public async Task UnbanUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);
        
        if (!DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Banned, out var banRelationId))
        {
            Logger.Warning(RelationDoesNotExistLogMessageGenerator
                .GenerateMessage(userId, topicId, UserTopicRelationStatus.Banned));
            
            throw new RelationDoesNotExistException(userId, topicId, UserTopicRelationStatus.Banned);
        }
        
        await userTopicRelationRepository.DeleteAsync(banRelationId, cancellationToken);
    }

    public async Task PromoteUserToModeratorAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);

        if (DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Banned, out var banRelationId))
        {
            Logger.Warning(UserBannedLogMessageGenerator.GenerateMessage(userId, topicId));
            
            throw new UserBannedException(userId, topicId);
        }
        
        if (DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Moderator, out var moderationRelationId))
        {
            Logger.Warning(UserAlreadyHasRelationLogMessageGenerator
                .GenerateMessage(userId, topicId, UserTopicRelationStatus.Moderator));
            
            throw new RelationAlreadyExistsException(userId, topicId, UserTopicRelationStatus.Moderator);
        }
        
        var relationModel = new UserTopicRelationModel()
        {
            UserId = userId,
            TopicId = topicId,
            TopicRelationStatus = UserTopicRelationStatus.Moderator
        };
        
        var relationEntity = Mapper.Map<UserTopicRelation>(relationModel);
        
        await userTopicRelationRepository.CreateAsync(relationEntity, cancellationToken);
    }

    public async Task DemoteUserFromModeratorAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);

        if (DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Banned, out var banRelationId))
        {
            Logger.Warning(UserBannedLogMessageGenerator.GenerateMessage(userId, topicId));
            
            throw new UserBannedException(userId, topicId);
        }
        
        if (!DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Moderator, out var moderationRelationId))
        {
            Logger.Warning(RelationDoesNotExistLogMessageGenerator
                .GenerateMessage(userId, topicId, UserTopicRelationStatus.Moderator));
            
            throw new RelationDoesNotExistException(userId, topicId, UserTopicRelationStatus.Moderator);
        }
        
        await userTopicRelationRepository.DeleteAsync(moderationRelationId, cancellationToken);
    }

    private bool DoesUserHaveRelationStatus
        (List<UserTopicRelationModel> relations, UserTopicRelationStatus targetStatus, out Guid relationId)
    {
        var subRelation = relations
            .FirstOrDefault(relation => relation.TopicRelationStatus == targetStatus);

        relationId = subRelation?.Id ?? Guid.Empty;

        return relationId != Guid.Empty;
    }
}
