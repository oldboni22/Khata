using AutoMapper;
using Serilog;
using Shared.Enums;
using Shared.Extensions;
using Shared.PagedList;
using UserService.BLL.Exceptions;
using UserService.BLL.Exceptions.Relations;
using UserService.BLL.Models;
using UserService.BLL.Models.User;
using UserService.BLL.Utilities.MessageGenerators.Logs;
using UserService.DAL.Models.Entities;
using UserService.DAL.Repositories;
using UserService.BLL.Utilities.MessageGenerators.Exceptions;

namespace UserService.BLL.Services;

public interface IUserService : IGenericService<User, UserModel, UserCreateModel, UserUpdateModel>
{
    Task<UserModel?> UpdateAsync(string senderId, Guid userId, UserUpdateModel updateModel, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(string senderId ,Guid userId, CancellationToken cancellationToken = default);
    
    Task<PagedList<UserModel>> FindUsersByTopicIdAsync(
        Guid topicId, UserTopicRelationStatus status, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);
    
    Task<PagedList<UserTopicRelationModel>> FindUserRelationsAsync(
        Guid userId, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);
    
    Task<List<Guid>> FindBannedTopicsIds(Guid userId, CancellationToken cancellationToken = default);
    
    Task<bool> DoesUserHaveTopicStatusAsync(Guid userId, Guid topicId, UserTopicRelationStatus status, CancellationToken cancellationToken = default);
    
    Task AddSubscriptionAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task RemoveSubscriptionAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task AddBanAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task RemoveBanAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task AddModerationStatusAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default);
    
    Task RemoveModerationStatusAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default);
}

public class UserService(
    IUserRepository userRepository,
    IUserTopicRelationRepository userTopicRelationRepository,
    IMapper mapper, 
    ILogger logger) : 
    GenericService<User, UserModel, UserCreateModel, UserUpdateModel>(userRepository, mapper, logger), IUserService
{
    public async Task<UserModel?> UpdateAsync(
        string senderId, 
        Guid userId, 
        UserUpdateModel updateModel,
        CancellationToken cancellationToken = default)
    {
        await ValidateSenderIdAsync(senderId, userId, cancellationToken);

        updateModel.Auth0Id = senderId;
        
        return await UpdateAsync(userId, updateModel, cancellationToken);
    }

    public async Task DeleteAsync(string senderId, Guid userId, CancellationToken cancellationToken = default)
    {
        await ValidateSenderIdAsync(senderId, userId, cancellationToken);
        
        await DeleteAsync(userId, cancellationToken);
    }

    public async Task<PagedList<UserModel>> FindUsersByTopicIdAsync(
        Guid topicId, 
        UserTopicRelationStatus status, 
        PaginationParameters paginationParameters, 
        CancellationToken cancellationToken = default)
    {
        var pagedRelations = await userTopicRelationRepository.FindByConditionAsync
            (
                relation => relation.TopicId == topicId && relation.TopicRelationStatus == status,
                paginationParameters,
                false,
                cancellationToken
            );
        
        var pagedUsers = pagedRelations
            .Items
            .Select(relation => relation.User)
            .ToList()
            .ToPagedList(paginationParameters.PageNumber, paginationParameters.PageSize, pagedRelations.PageCount);
        
        return Mapper.Map<PagedList<UserModel>>(pagedUsers);
    }

    public async Task<PagedList<UserTopicRelationModel>> FindUserRelationsAsync(
        Guid userId, PaginationParameters paginationParameters, CancellationToken cancellationToken = default)
    {
        var relationEntities = await userTopicRelationRepository
            .FindByConditionAsync
                (
                    relation => relation.UserId == userId,
                    paginationParameters,
                    false,
                    cancellationToken
                );
        
        return Mapper.Map<PagedList<UserTopicRelationModel>>(relationEntities);
    }

    public async Task<List<Guid>> FindBannedTopicsIds(Guid userId, CancellationToken cancellationToken = default)
    {
        var relations = await userTopicRelationRepository
            .FindAllByConditionAsync
            (
                relation => relation.UserId == userId && relation.TopicRelationStatus == UserTopicRelationStatus.Banned,
                false,
                cancellationToken
            );
        
        return relations.Select(r => r.TopicId).ToList();
    }

    public async Task<bool> DoesUserHaveTopicStatusAsync(
        Guid userId, Guid topicId, UserTopicRelationStatus status, CancellationToken cancellationToken = default)
    {
        var relations = await FindUserTopicRelationsAsync(userId, topicId, cancellationToken);
        
        return DoesUserHaveRelationStatus(relations, status, out var relationId);
    }

    public async Task AddSubscriptionAsync(
        string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        await ValidateSenderIdAsync(senderId, userId, cancellationToken);
        
        var relationModels = await FindUserTopicRelationsAsync(userId, topicId, cancellationToken);
        
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

        var userModel = await FindByIdAsync(userId, cancellationToken);
        
        var relation = new UserTopicRelationModel()
        {
            UserId = userId,
            TopicId = topicId,
            TopicRelationStatus = UserTopicRelationStatus.Subscribed,
            User = userModel!
        };

        var relationEntity = Mapper.Map<UserTopicRelation>(relation);
        
        await userTopicRelationRepository.CreateAsync(relationEntity, cancellationToken);
    }

    public async Task RemoveSubscriptionAsync(
        string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        await ValidateSenderIdAsync(senderId, userId, cancellationToken);
        
        var relationModels = await FindUserTopicRelationsAsync(userId, topicId, cancellationToken);
        
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

    public async Task AddBanAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var senderUser = await GetUserByAuth0IdAsync(senderId, cancellationToken);
        
        var senderRelationModels = await FindUserTopicRelationsAsync(senderUser.Id, topicId, cancellationToken);
        
        if(!DoesUserHaveRelationStatus(senderRelationModels, UserTopicRelationStatus.Moderator, out var moderRelationId))
        {
            Logger.Warning(ForbiddenLogMessageGenerator.GenerateMessage(senderUser.Id));

            throw new ForbiddenException(senderUser.Id);
        }

        if (senderUser.Id == userId)
        {
            throw new BadRequestException(SelfBanExceptionMessages.DefaultMessage);
        }
        
        var relationModels = await FindUserTopicRelationsAsync(userId, topicId, cancellationToken);

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
        
        var userModel = await FindByIdAsync(userId, cancellationToken);
        
        var relationModel = new UserTopicRelationModel()
        {
            UserId = userId,
            TopicId = topicId,
            TopicRelationStatus = UserTopicRelationStatus.Banned,
            User = userModel!
        };
        
        var relationEntity = Mapper.Map<UserTopicRelation>(relationModel);
        
        await userTopicRelationRepository.CreateAsync(relationEntity, cancellationToken);
    }

    public async Task RemoveBanAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var senderUser = await GetUserByAuth0IdAsync(senderId, cancellationToken);
        
        var senderRelationModels = await FindUserTopicRelationsAsync(senderUser.Id, topicId, cancellationToken);
        
        if(!DoesUserHaveRelationStatus(senderRelationModels, UserTopicRelationStatus.Moderator, out var moderRelationId))
        {
            Logger.Warning(ForbiddenLogMessageGenerator.GenerateMessage(senderUser.Id));

            throw new ForbiddenException(senderUser.Id);
        }
        
        var relationModels = await FindUserTopicRelationsAsync(userId, topicId, cancellationToken);
        
        if (!DoesUserHaveRelationStatus(relationModels, UserTopicRelationStatus.Banned, out var banRelationId))
        {
            Logger.Warning(RelationDoesNotExistLogMessageGenerator
                .GenerateMessage(userId, topicId, UserTopicRelationStatus.Banned));
            
            throw new RelationDoesNotExistException(userId, topicId, UserTopicRelationStatus.Banned);
        }
        
        await userTopicRelationRepository.DeleteAsync(banRelationId, cancellationToken);
    }

    public async Task AddModerationStatusAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        //TODO When topic service will be ready check is sender a topic owner
        
        var relationModels = await FindUserTopicRelationsAsync(userId, topicId, cancellationToken);

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
        
        var userModel = await FindByIdAsync(userId, cancellationToken);
        
        var relationModel = new UserTopicRelationModel()
        {
            UserId = userId,
            TopicId = topicId,
            TopicRelationStatus = UserTopicRelationStatus.Banned,
            User = userModel!
        };
        
        var relationEntity = Mapper.Map<UserTopicRelation>(relationModel);
        
        await userTopicRelationRepository.CreateAsync(relationEntity, cancellationToken);
    }

    public async Task RemoveModerationStatusAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        //TODO When topic service will be ready check is sender a topic owner
        
        var relationModels = await FindUserTopicRelationsAsync(userId, topicId, cancellationToken);

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

    private async Task<List<UserTopicRelationModel>> FindUserTopicRelationsAsync(
        Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        if (!await userRepository.ExistsAsync(userId, cancellationToken))
        {
            throw new EntityNotFoundException<User>(userId);
        }
        
        var pagedList = await userTopicRelationRepository
            .FindByConditionAsync
                (
                    ent => ent.UserId == userId && ent.TopicId == topicId,
                    new PaginationParameters(1,5),
                    false,
                    cancellationToken
                );

        return Mapper.Map<List<UserTopicRelationModel>>(pagedList.Items);
    }
    
    private bool DoesUserHaveRelationStatus(
        List<UserTopicRelationModel> relations, UserTopicRelationStatus targetStatus, out Guid relationId)
    {
        var relation = relations
            .FirstOrDefault(relation => relation.TopicRelationStatus == targetStatus);

        relationId = relation?.Id ?? Guid.Empty;

        return relationId != Guid.Empty;
    }

    private async Task<User> GetUserByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken)
    {
        var userEntity = await userRepository.FindUserByAuth0IdAsync(auth0Id, cancellationToken);

        if (userEntity is null)
        {
            throw new NotFoundException(Auth0IdNotFoundExceptionMessageGenerator.GenerateMessage(auth0Id));
        }

        return userEntity;
    }
    
    private async Task ValidateSenderIdAsync(string senderId, Guid userId, CancellationToken cancellationToken)
    {
        var userEntity = await GetUserByAuth0IdAsync(senderId, cancellationToken);

        if (userEntity.Id != userId)
        {
            throw new ForbiddenException(userEntity.Id);
        }
    }
}
