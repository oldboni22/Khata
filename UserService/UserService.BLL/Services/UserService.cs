using AutoMapper;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel;
using MinIoService;
using Serilog;
using Shared.Enums;
using Shared.Extensions;
using Shared.PagedList;
using UserService.BLL.Exceptions;
using UserService.BLL.Exceptions.Relations;
using UserService.BLL.gRpc;
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

    Task UpdatePictureAsync(string senderId, Guid userId, IFormFile file, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(string senderId ,Guid userId, CancellationToken cancellationToken = default);
    
    Task DeletePictureAsync(string senderId, Guid userId, CancellationToken cancellationToken = default);

    Task<(Stream stream, ObjectStat stats)> FindUserPictureAsync(
        string senderId, Guid userId, CancellationToken cancellationToken = default);
    
    Task<PagedList<UserModel>> FindUsersByTopicIdAsync(
        Guid topicId, UserTopicRelationStatus status, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);
    
    Task<PagedList<UserTopicRelationModel>> FindUserRelationsAsync(
        Guid userId, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);
    
    Task<List<Guid>> FindBannedTopicsIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Guid> FindUserIdByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default);
    
    Task<List<string>> FindUsersWithStatusAsync(Guid topicId, UserTopicRelationStatus status, CancellationToken cancellationToken = default);
    
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
    ITopicGRpcClient topicGRpcClient,
    IMinioService minioService,
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

    public async Task UpdatePictureAsync(
        string senderId, Guid userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        await ValidateSenderIdAsync(senderId, userId, cancellationToken);

        var minioKey = userId.ToString();
        
        await minioService.UploadFileAsync(file, minioKey);
    }

    public async Task DeleteAsync(string senderId, Guid userId, CancellationToken cancellationToken = default)
    {
        await ValidateSenderIdAsync(senderId, userId, cancellationToken);
        
        await DeleteAsync(userId, cancellationToken);
    }

    public async Task DeletePictureAsync(string senderId, Guid userId, CancellationToken cancellationToken = default)
    {
        await ValidateSenderIdAsync(senderId, userId, cancellationToken);

        var minioKey = userId.ToString();
        
        await minioService.DeleteFileAsync(minioKey);
    }

    public async Task<(Stream stream, ObjectStat stats)> FindUserPictureAsync(string senderId, Guid userId, CancellationToken cancellationToken = default)
    {
        await ValidateSenderIdAsync(senderId, userId, cancellationToken);

        var minioKey = userId.ToString();

        var minioResult = await minioService.GetFileAsync(minioKey);

        if (minioResult is null)
        {
            throw new MediaNotFoundException(userId);
        }

        return minioResult.Value;
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
            .ToPagedList(paginationParameters.PageNumber, paginationParameters.PageSize, pagedRelations.PageCount, pagedRelations.TotalCount);
        
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

    public async Task<List<Guid>> FindBannedTopicsIdsAsync(Guid userId, CancellationToken cancellationToken = default)
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

    public async Task<Guid> FindUserIdByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken = default)
    {
        var user = await FindUserByAuth0IdAsync(auth0Id, cancellationToken);

        return user.Id;
    }

    public async Task<List<string>> FindUsersWithStatusAsync(
        Guid topicId, UserTopicRelationStatus status, CancellationToken cancellationToken = default)
    {
        var relations = await userTopicRelationRepository
            .FindAllByConditionAsync(relation => 
                relation.TopicId == topicId && relation.TopicRelationStatus == status, false, cancellationToken);
        
        return relations
            .Select(rel => rel.UserId.ToString())
            .ToList();
    }

    public async Task<bool> DoesUserHaveTopicStatusAsync(
        Guid userId, Guid topicId, UserTopicRelationStatus status, CancellationToken cancellationToken = default)
    {
        var relation = await FindUserTopicRelationAsync(userId, topicId, cancellationToken);
        
        return relation?.TopicRelationStatus == status;
    }

    public async Task AddSubscriptionAsync(
        string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        await ValidateSenderIdAsync(senderId, userId, cancellationToken);
        
        var relation = await FindUserTopicRelationAsync(userId, topicId, cancellationToken);

        if (relation is not null)
        {
            switch (relation.TopicRelationStatus)
            {
                case UserTopicRelationStatus.Subscribed: 
                    throw new RelationAlreadyExistsException(userId, topicId, UserTopicRelationStatus.Subscribed);
                
                case UserTopicRelationStatus.Moderator:
                    throw new RelationAlreadyExistsException(userId, topicId, UserTopicRelationStatus.Moderator);
                
                case UserTopicRelationStatus.Banned:
                    throw new UserBannedException(userId, topicId);
            }
        }

        var relationEntity = new UserTopicRelation
        {
            UserId = userId,
            TopicId = topicId,
            User = null!
        };
        
        await userTopicRelationRepository.CreateAsync(relationEntity, cancellationToken);
    }

    public async Task RemoveSubscriptionAsync(
        string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        await ValidateSenderIdAsync(senderId, userId, cancellationToken);
        
        var relation = await FindUserTopicRelationAsync(userId, topicId, cancellationToken)
                       ?? throw new RelationDoesNotExistException(userId, topicId, UserTopicRelationStatus.Subscribed);

        if (relation.TopicRelationStatus != UserTopicRelationStatus.Subscribed)
        {
            throw new RelationDoesNotExistException(userId, topicId, UserTopicRelationStatus.Subscribed);
        }
        
        await userTopicRelationRepository.DeleteAsync(relation.Id, cancellationToken);
    }

    public async Task AddBanAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var senderUser = await FindUserByAuth0IdAsync(senderId, cancellationToken);
        var senderRelation = await FindUserTopicRelationAsync(senderUser.Id, topicId, cancellationToken);
        
        var hasModeration = senderRelation?.TopicRelationStatus == UserTopicRelationStatus.Moderator;
        var isOwner = await topicGRpcClient.IsOwnerAsync(senderUser.Id, topicId);
        
        if(!isOwner && !hasModeration)
        {
            Logger.Warning(ForbiddenLogMessageGenerator.GenerateMessage(senderUser.Id));

            throw new ForbiddenException(senderUser.Id);
        }

        if (senderUser.Id == userId)
        {
            throw new BadRequestException(SelfBanExceptionMessages.DefaultMessage);
        }

        var relation = await FindUserTopicRelationAsync(userId, topicId, cancellationToken);

        if (relation is null)
        {
            await userTopicRelationRepository.CreateAsync
            (new UserTopicRelation
            {
                UserId = userId,
                TopicId = topicId,
                TopicRelationStatus = UserTopicRelationStatus.Banned
            }, cancellationToken);
            
            return;
        }

        if (relation.TopicRelationStatus == UserTopicRelationStatus.Moderator && !isOwner)
        {
            throw new ForbiddenException(senderUser.Id);
        }
        
        relation.TopicRelationStatus = UserTopicRelationStatus.Banned;
        
        await userTopicRelationRepository.UpdateAsync(relation, cancellationToken);
    }

    public async Task RemoveBanAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var senderUser = await FindUserByAuth0IdAsync(senderId, cancellationToken);
        var senderRelation = await FindUserTopicRelationAsync(senderUser.Id, topicId, cancellationToken);
        
        var hasModeration = senderRelation?.TopicRelationStatus == UserTopicRelationStatus.Moderator;
        var isOwner = await topicGRpcClient.IsOwnerAsync(senderUser.Id, topicId);
        
        if(!isOwner && !hasModeration)
        {
            Logger.Warning(ForbiddenLogMessageGenerator.GenerateMessage(senderUser.Id));

            throw new ForbiddenException(senderUser.Id);
        }
        
        var relation = await FindUserTopicRelationAsync(userId, topicId, cancellationToken);

        if (relation is null || relation.TopicRelationStatus != UserTopicRelationStatus.Banned)
        {
            throw new RelationDoesNotExistException(userId, topicId, UserTopicRelationStatus.Banned);
        }
        
        await userTopicRelationRepository.DeleteAsync(relation.Id, cancellationToken);
    }

    public async Task AddModerationStatusAsync(string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var senderUser = await FindUserByAuth0IdAsync(senderId, cancellationToken);

        if (!await topicGRpcClient.IsOwnerAsync(senderUser.Id, topicId))
        {
            throw new ForbiddenException(senderUser.Id);
        }
        
        var relation = await FindUserTopicRelationAsync(userId, topicId, cancellationToken);

        if (relation is null)
        {
            var createdRelation = new UserTopicRelation()
            {
                UserId = userId,
                TopicId = topicId,
                TopicRelationStatus = UserTopicRelationStatus.Moderator,
            };
            
            await userTopicRelationRepository.CreateAsync(createdRelation, cancellationToken);
            
            return;
        }
        
        relation.TopicRelationStatus = UserTopicRelationStatus.Moderator;
        
        await userTopicRelationRepository.UpdateAsync(relation, cancellationToken);
    }

    public async Task RemoveModerationStatusAsync(
        string senderId, Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var senderUser = await FindUserByAuth0IdAsync(senderId, cancellationToken);

        if (!await topicGRpcClient.IsOwnerAsync(senderUser.Id, topicId))
        {
            throw new ForbiddenException(senderUser.Id);
        }
        
        var relation = await FindUserTopicRelationAsync(userId, topicId, cancellationToken)
                       ?? throw new RelationDoesNotExistException(userId, topicId, UserTopicRelationStatus.Moderator);
        
        await userTopicRelationRepository.DeleteAsync(relation.Id, cancellationToken);
    }

    private async Task<UserTopicRelation?> FindUserTopicRelationAsync(
        Guid userId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var target = await FindByIdAsync(userId, cancellationToken);
        
        if (target is null)
        {
            throw new EntityNotFoundException<User>(userId);
        }
        
        var list = await userTopicRelationRepository
            .FindAllByConditionAsync
                (
                    ent => ent.UserId == userId && ent.TopicId == topicId,
                    true,
                    cancellationToken
                );
        
        return list.FirstOrDefault();
    }
    
    private async Task<User> FindUserByAuth0IdAsync(string auth0Id, CancellationToken cancellationToken)
    {
        var userEntity = await userRepository.FindUserByAuth0IdAsync(auth0Id, cancellationToken)
            ?? throw new NotFoundException(Auth0IdNotFoundExceptionMessageGenerator.GenerateMessage(auth0Id));

        return userEntity;
    }
    
    private async Task ValidateSenderIdAsync(string senderId, Guid userId, CancellationToken cancellationToken)
    {
        var userEntity = await userRepository.FindByIdAsync(userId,false, cancellationToken)
            ?? throw new EntityNotFoundException<User>(userId);

        if (userEntity.Auth0Id != senderId)
        {
            throw new ForbiddenException(userEntity.Id);
        }
    }
}
