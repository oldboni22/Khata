using AutoMapper;
using Serilog;
using UserService.BLL.Exceptions;
using UserService.BLL.Exceptions.Relations;
using UserService.BLL.Models;
using UserService.BLL.Models.User;
using UserService.DAL.Models.Entities;
using UserService.DAL.Models.Enums;
using UserService.DAL.Repositories;

namespace UserService.BLL.Services;

public interface IUserService : IGenericService<User, UserModel, UserCreateModel, UserUpdateModel>
{
    Task SubscribeUser(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task UnsubscribeUser(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task BanUser(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task UnbanUser(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task PromoteUserToModerator(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task DemoteUserFromModerator(Guid userId, Guid topicId, CancellationToken cancellationToken = default);
}

public class UserService(IGenericRepository<User> repository, UserTopicRelationRepository userTopicRelationRepository, IMapper mapper, ILogger? logger) : 
    GenericService<User, UserModel, UserCreateModel, UserUpdateModel>(repository, mapper, logger), IUserService
{
    public async Task SubscribeUser(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);
        
        if (IsUserSubscribedToTopic(relationModels, out var subscriptionRelationId))
        {
            Logger?.Warning($"A user with id {userId} is already subscribed to topic with id {topicId}.");
            
            throw new RelationAlreadyExistsException(userId, topicId, UserTopicRelationStatus.Subscribed);
        }
        
        if (IsUserBannedFromTopic(relationModels, out var banRelationId))
        {
            Logger?.Warning($"A user with id {userId} is banned from topic with id {topicId}.");

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

    public async Task UnsubscribeUser(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);
        
        if (! IsUserSubscribedToTopic(relationModels, out var subscriptionRelationId))
        {
            Logger?.Warning($"A user with id {userId} was not subscribed to topic with id {topicId}.");
            
            throw new RelationDoesNotExist(userId, topicId, UserTopicRelationStatus.Subscribed);
        }
        
        if (IsUserBannedFromTopic(relationModels, out var banRelationId))
        {
            Logger?.Warning($"User with id {userId} is banned from topic with id {topicId}, cannot unsubscribe.");
            
            throw new UserBannedException(userId, topicId);
        }
        
        var relationModel = relationModels.First(relation => relation.TopicRelationStatus == UserTopicRelationStatus.Subscribed);
        
        await userTopicRelationRepository.DeleteAsync(relationModel.Id, cancellationToken);
    }

    public async Task BanUser(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);

        if (IsUserSubscribedToTopic(relationModels, out var subscriptionRelationId))
        {
            Logger?.Information($"User with id {userId} is subscribed to the topic with id {topicId}, removing subscription before banning.");
            
            await userTopicRelationRepository.DeleteAsync(subscriptionRelationId, cancellationToken);
        }

        if (IsUserModeratingTopic(relationModels, out var moderationRelationId))
        {
            Logger?.Information($"User with id {userId} is moderating the topic with id {topicId}, removing moderation before banning.");
            
            await userTopicRelationRepository.DeleteAsync(moderationRelationId, cancellationToken);
        }
        
        if (IsUserBannedFromTopic(relationModels, out var banRelationId))
        {
            Logger?.Warning($"User with id {userId} is already banned from topic with id {topicId}.");
            
            throw new RelationAlreadyExistsException(userId, topicId, UserTopicRelationStatus.Banned);
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

    public async Task UnbanUser(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);
        
        if (! IsUserBannedFromTopic(relationModels, out var banRelationId))
        {
            Logger?.Warning($"A user with id {userId} is not banned from topic with id {topicId}.");
            
            throw new RelationDoesNotExist(userId, topicId, UserTopicRelationStatus.Banned);
        }
        
        await userTopicRelationRepository.DeleteAsync(banRelationId, cancellationToken);
    }

    public async Task PromoteUserToModerator(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);

        if (IsUserBannedFromTopic(relationModels, out var banRelationId))
        {
            Logger?.Warning($"User with id {userId} is banned from topic with id {topicId}, cannot promote to moderator.");
            
            throw new UserBannedException(userId, topicId);
        }
        
        if (IsUserModeratingTopic(relationModels, out var moderationRelationId))
        {
            Logger?.Warning($"User with id {userId} is already moderating topic with id {topicId}.");
            
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

    public async Task DemoteUserFromModerator(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync(ent => ent.UserId == userId && ent.TopicId == topicId, false, cancellationToken);

        var relationModels = Mapper.Map<List<UserTopicRelationModel>>(relationEntities);

        if (IsUserBannedFromTopic(relationModels, out var banRelationId))
        {
            Logger?.Warning($"User with id {userId} is banned from topic with id {topicId}, cannot demote from moderator.");
            
            throw new UserBannedException(userId, topicId);
        }
        
        if (! IsUserModeratingTopic(relationModels, out var moderationRelationId))
        {
            Logger?.Warning($"User with id {userId} is not moderating topic with id {topicId}.");
            
            throw new RelationDoesNotExist(userId, topicId, UserTopicRelationStatus.Moderator);
        }
        
        await userTopicRelationRepository.DeleteAsync(moderationRelationId, cancellationToken);
    }

    private bool IsUserSubscribedToTopic(List<UserTopicRelationModel> relations, out Guid relationId)
    {
        var subRelation = relations.FirstOrDefault(relation => relation.TopicRelationStatus == UserTopicRelationStatus.Subscribed);

        relationId = subRelation == null ? Guid.Empty : subRelation.Id;

        return subRelation != null;
    }
    
    private bool IsUserBannedFromTopic(List<UserTopicRelationModel> relations, out Guid relationId)
    {
        var banRelation = relations.FirstOrDefault(relation => relation.TopicRelationStatus == UserTopicRelationStatus.Banned);

        relationId = banRelation == null ? Guid.Empty : banRelation.Id;

        return banRelation != null;
    }
    
    private bool IsUserModeratingTopic(List<UserTopicRelationModel> relations, out Guid relationId)
    {
        var modRelation = relations.FirstOrDefault(relation => relation.TopicRelationStatus == UserTopicRelationStatus.Moderator);

        relationId = modRelation == null ? Guid.Empty : modRelation.Id;

        return modRelation != null;
    }
}