using AutoMapper;
using Serilog;
using Shared.Enums;
using Shared.PagedList;
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
    Task<PagedList<UserModel>> FindUsersByTopicIdAsync(
        Guid topicId, UserTopicRelationStatus status, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);
    
    Task<PagedList<UserTopicRelationModel>> FindUserRelationsAsync(
        Guid userId, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);
    
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

    public async Task SubscribeUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
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

    public async Task UnsubscribeUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
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

    public async Task BanUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
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

    public async Task UnbanUserAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var relationModels = await FindUserTopicRelationsAsync(userId, topicId, cancellationToken);
        
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

    public async Task DemoteUserFromModeratorAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
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

    private async Task<List<UserTopicRelationModel>> FindUserTopicRelationsAsync(Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        if (!await Repository.ExistsAsync(userId, cancellationToken))
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
    
    private bool DoesUserHaveRelationStatus
        (List<UserTopicRelationModel> relations, UserTopicRelationStatus targetStatus, out Guid relationId)
    {
        var subRelation = relations
            .FirstOrDefault(relation => relation.TopicRelationStatus == targetStatus);

        relationId = subRelation?.Id ?? Guid.Empty;

        return relationId != Guid.Empty;
    }
}
