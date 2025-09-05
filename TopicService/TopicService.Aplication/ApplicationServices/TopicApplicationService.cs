using System.Linq.Expressions;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.RepositoryContracts;
using Infrastructure.gRpc;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Filters;
using Shared.Filters.Topic;
using Shared.PagedList;
using TopicService.API.Dto.Post;
using TopicService.API.Dto.Topic;
using TopicService.API.Utilities.LogMessages;
using ILogger = Serilog.ILogger;
using TopicReadDto = Domain.Entities.TopicReadDto;

namespace TopicService.API.ApplicationServices;

public interface ITopicApplicationService
{
    Task<Dto.Topic.TopicReadDto> CreateHeadTopicAsync(
        string senderId, TopicCreateDto topicCreateDto, CancellationToken cancellationToken = default);

    Task RemoveHeadTopicAsync(string senderId, Guid topicId, CancellationToken cancellationToken = default);

    Task<Dto.Topic.TopicReadDto> CreateSubTopicAsync(
       string senderId, TopicCreateDto topicCreateDto, Guid parentTopicId, CancellationToken cancellationToken = default);
    
    Task RemoveSubTopicAsync(
        string senderId, Guid parentTopicId, Guid topicId, CancellationToken cancellationToken = default); 
    
    Task<TopicReadDto> UpdateTopicOwnerAsync(
        string senderId, Guid topicId, Guid userId, CancellationToken cancellationToken = default);
    
    //перенести в сервис постов
    Task<PostReadDto> CreatePostAsync(
        string senderId, PostCreateDto postCreateDto, Guid topicId, CancellationToken cancellationToken = default);
    
    Task RemovePostAsync(
        string senderId, Guid postId, Guid topicId, CancellationToken cancellationToken = default);
    //
    
    Task<PagedList<TopicReadDto>> FindHeadTopics(
        TopicSearchParameters parameters, 
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default);
    
    Task<PagedList<TopicReadDto>> FindChildTopics(
        Guid parentTopicId,
        TopicSearchParameters parameters, 
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default);
}

public class TopicTopicApplicationService(
    ITopicRepository repository, IUserGRpcClient userGRpcClient, IMapper mapper, ILogger logger) : 
    ApplicationServiceBase<TopicReadDto, TopicSortOptions>(repository, userGRpcClient, mapper, logger),ITopicApplicationService
{
    public async Task<Dto.Topic.TopicReadDto> CreateHeadTopicAsync(string senderId, TopicCreateDto topicCreateDto, CancellationToken cancellationToken = default)
    {
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var topicEntity = TopicReadDto.Create(topicCreateDto.Name, senderUserId);
        
        var createdTopic = await Repository.CreateAsync(topicEntity, cancellationToken);
        
        return Mapper.Map<Dto.Topic.TopicReadDto>(createdTopic);
    }

    public async Task RemoveHeadTopicAsync(string senderId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var topic = await Repository.FindByIdAsync(topicId, cancellationToken: cancellationToken);

        if (topic is null)
        {
            ThrowNotFoundException<TopicReadDto>(topicId);
        }

        if (senderUserId != topic!.OwnerId)
        {
            throw new ForbiddenException();
        }
        
        await Repository.DeleteAsync(topicId, cancellationToken);
    }

    public async Task<Dto.Topic.TopicReadDto> CreateSubTopicAsync(
        string senderId, TopicCreateDto topicCreateDto, Guid parentTopicId, CancellationToken cancellationToken = default)
    {
        var parentTopic = await Repository
            .FindByIdAsync(parentTopicId, true, cancellationToken);

        if (parentTopic is null)
        {
            ThrowNotFoundException<TopicReadDto>(parentTopicId);    
        }

        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var createdTopic = parentTopic!.AddSubTopic(topicCreateDto.Name, senderUserId);

        await Repository.SaveChangesAsync(cancellationToken);
        
        return Mapper.Map<Dto.Topic.TopicReadDto>(createdTopic);
    }

    public async Task RemoveSubTopicAsync(
        string senderId, Guid parentTopicId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await Repository
            .FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            ThrowNotFoundException<TopicReadDto>(topicId);
        }

        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        if (!await UserGRpcClient.IsModeratorAsync(senderUserId, topicId))
        {
            throw new ForbiddenException();
        }

        topic!.RemoveSubTopic(topicId, senderUserId);
        
        await Repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<TopicReadDto> UpdateTopicOwnerAsync(string senderId, Guid topicId, Guid userId, CancellationToken cancellationToken = default)
    {
        var topic = await Repository
            .FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            ThrowNotFoundException<TopicReadDto>(topicId);
        }
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);

        if (senderUserId == topic!.OwnerId)
        {
            throw new ForbiddenException();
        }
        
        topic!.SetOwner(userId);
        await Repository.SaveChangesAsync(cancellationToken);
        
        return Mapper.Map<TopicReadDto>(topic);
    }


    public async Task<PostReadDto> CreatePostAsync(
        string senderId,PostCreateDto postCreateDto, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await Repository.FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            ThrowNotFoundException<TopicReadDto>(topicId);
        }

        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var createdPost = topic!.AddPost(postCreateDto.Title, postCreateDto.Text, senderUserId);
        
        await Repository.SaveChangesAsync(cancellationToken);
        
        return Mapper.Map<PostReadDto>(createdPost);
    }

    public async Task RemovePostAsync(string senderId, Guid postId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await Repository.FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            ThrowNotFoundException<TopicReadDto>(topicId);
        }

        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        if (!await UserGRpcClient.IsModeratorAsync(senderUserId, topicId))
        {
            throw new ForbiddenException();
        }
        
        topic!.RemovePost(postId, senderUserId);
        
        await Repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedList<TopicReadDto>> FindHeadTopics(
        TopicSearchParameters parameters, 
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<TopicReadDto, bool>> expression = t => t.ParentTopicId == null;

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            expression = expression.And(t =>
                t.Name.Contains(parameters.SearchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
        
        var selectors = ParseFilters(parameters.Filters);

        return await Repository
            .FindByConditionWithFilterAsync
            (
                expression,
                selectors,
                paginationParameters,
                false,
                cancellationToken
            );
    }

    public async Task<PagedList<TopicReadDto>> FindChildTopics(
        Guid parentTopicId,
        TopicSearchParameters parameters,
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        if (!await Repository.ExistsAsync(parentTopicId, cancellationToken))
        {
            throw new EntityNotFoundException<TopicReadDto>(parentTopicId);
        }
        
        Expression<Func<TopicReadDto, bool>> expression = t => t.ParentTopicId == parentTopicId;

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            expression = expression.And(t =>
                t.Name.Contains(parameters.SearchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
        
        var selectors = ParseFilters(parameters.Filters);
        
        return await Repository
            .FindByConditionWithFilterAsync
            (
                expression,
                selectors,
                paginationParameters,
                false,
                cancellationToken
            );
    }
    
    private void ThrowNotFoundException<T>(Guid id) where T : EntityBase
    {
        Logger.Information(EntityNotFoundLogMessage<T>.Generate(id));
        
        throw new EntityNotFoundException<T>(id);
    }

    protected override Expression<Func<TopicReadDto, object>> ParseSortOptions(TopicSortOptions sortOptions)
    {
        return sortOptions switch
        {
            TopicSortOptions.CreateDate => t => t.CreatedAt,
            TopicSortOptions.PostCount => t => t.Posts.Count,
            _ => DefaultSortOptions.selector,
        };
    }

    protected override (Expression<Func<TopicReadDto, object>> selector, bool ascending) DefaultSortOptions
    {
        get
        {
            return (t => t.Name, true);
        }
    }
}
