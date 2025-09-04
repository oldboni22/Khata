using System.Linq.Expressions;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.RepositoryContracts;
using Infrastructure.gRpc;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Filters;
using Shared.PagedList;
using TopicService.API.Dto.Post;
using TopicService.API.Dto.Topic;
using TopicService.API.Utilities.LogMessages;
using ILogger = Serilog.ILogger;

namespace TopicService.API.ApplicationServices;

public interface ITopicApplicationService
{
    Task<TopicReadDto> CreateHeadTopicAsync(string senderId, TopicCreateDto topicCreateDto, CancellationToken cancellationToken = default);

    Task RemoveHeadTopicAsync(string senderId, Guid topicId, CancellationToken cancellationToken = default);

    Task<TopicReadDto> CreateSubTopicAsync(
       string senderId, TopicCreateDto topicCreateDto, Guid parentTopicId, CancellationToken cancellationToken = default);
    
    Task RemoveSubTopicAsync(string senderId, Guid parentTopicId, Guid topicId, CancellationToken cancellationToken = default); 
    
    //перенести в сервис постов
    Task<PostReadDto> CreatePostAsync(string senderId, PostCreateDto postCreateDto, Guid topicId, CancellationToken cancellationToken = default);
    
    Task RemovePostAsync(string senderId, Guid postId, Guid topicId, CancellationToken cancellationToken = default);
    //
    
    Task<PagedList<Topic>> FindHeadTopics(
        TopicSearchParameters parameters, 
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default);
    
    Task<PagedList<Topic>> FindChildTopics(
        Guid parentTopicId,
        TopicSearchParameters parameters, 
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default);
}

public class TopicTopicApplicationService(
    ITopicRepository topicRepository, IUserGRpcClient userGRpcClient, IMapper mapper, ILogger logger) : 
    ApplicationServiceBase<Topic, TopicSortOptions>(topicRepository, userGRpcClient, mapper, logger),ITopicApplicationService
{
    public async Task<TopicReadDto> CreateHeadTopicAsync(string senderId, TopicCreateDto topicCreateDto, CancellationToken cancellationToken = default)
    {
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var topicEntity = Topic.Create(topicCreateDto.Name, senderUserId);
        
        var createdTopic = await TopicRepository.CreateAsync(topicEntity, cancellationToken);
        
        return Mapper.Map<TopicReadDto>(createdTopic);
    }

    public async Task RemoveHeadTopicAsync(string senderId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var topic = await TopicRepository.FindByIdAsync(topicId, cancellationToken: cancellationToken);

        if (topic is null)
        {
            ThrowNotFoundException<Topic>(topicId);
        }

        if (senderUserId != topic!.OwnerId)
        {
            throw new ForbiddenException();
        }
        
        await TopicRepository.DeleteAsync(topicId, cancellationToken);
    }

    public async Task<TopicReadDto> CreateSubTopicAsync(
        string senderId, TopicCreateDto topicCreateDto, Guid parentTopicId, CancellationToken cancellationToken = default)
    {
        var parentTopic = await TopicRepository
            .FindByIdAsync(parentTopicId, true, cancellationToken);

        if (parentTopic is null)
        {
            ThrowNotFoundException<Topic>(parentTopicId);    
        }

        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var createdTopic = parentTopic!.AddSubTopic(topicCreateDto.Name, senderUserId);

        await TopicRepository.SaveChangesAsync(cancellationToken);
        
        return Mapper.Map<TopicReadDto>(createdTopic);
    }

    public async Task RemoveSubTopicAsync(
        string senderId, Guid parentTopicId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await TopicRepository
            .FindByIdAsync(topicId, true, cancellationToken);


        if(topic is null)
        {
            ThrowNotFoundException<Topic>(topicId);
        }

        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        if (!await UserGRpcClient.IsModeratorAsync(senderUserId, topicId))
        {
            throw new ForbiddenException();
        }

        topic!.RemoveSubTopic(topicId, senderUserId);
        
        await TopicRepository.SaveChangesAsync(cancellationToken);
    }


    public async Task<PostReadDto> CreatePostAsync(
        string senderId,PostCreateDto postCreateDto, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            ThrowNotFoundException<Topic>(topicId);
        }

        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var createdPost = topic!.AddPost(postCreateDto.Title, postCreateDto.Text, senderUserId);
        
        await TopicRepository.SaveChangesAsync(cancellationToken);
        
        return Mapper.Map<PostReadDto>(createdPost);
    }

    public async Task RemovePostAsync(string senderId, Guid postId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            ThrowNotFoundException<Topic>(topicId);
        }

        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        if (!await UserGRpcClient.IsModeratorAsync(senderUserId, topicId))
        {
            throw new ForbiddenException();
        }
        
        topic!.RemovePost(postId, senderUserId);
        
        await TopicRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedList<Topic>> FindHeadTopics(
        TopicSearchParameters parameters, 
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Topic, bool>> expression = t => t.ParentTopicId == null;

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            expression = expression.And(t =>
                t.Name.Contains(parameters.SearchTerm, StringComparison.InvariantCultureIgnoreCase));
        }

        var filters = parameters
            .Filters
            .Select(entry => (entry.SortOptions, entry.Ascending))
            .ToArray();
        
        var selectors = ParseFilters(filters);

        return await TopicRepository
            .FindByConditionWithFilterAsync
            (
                expression,
                selectors,
                paginationParameters,
                false,
                cancellationToken
            );
    }

    public async Task<PagedList<Topic>> FindChildTopics(
        Guid parentTopicId,
        TopicSearchParameters parameters,
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        if (!await TopicRepository.ExistsAsync(parentTopicId, cancellationToken))
        {
            throw new EntityNotFoundException<Topic>(parentTopicId);
        }
        
        Expression<Func<Topic, bool>> expression = t => t.ParentTopicId == parentTopicId;

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            expression = expression.And(t =>
                t.Name.Contains(parameters.SearchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
        
        var filters = parameters
            .Filters
            .Select(entry => (entry.SortOptions, entry.Ascending))
            .ToArray();
        
        var selectors = ParseFilters(filters);
        
        return await TopicRepository
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

    protected override Expression<Func<Topic, object>> ParseSortOptions(TopicSortOptions sortOptions)
    {
        return sortOptions switch
        {
            TopicSortOptions.CreateDate => t => t.CreatedAt,
            TopicSortOptions.PostCount => t => t.Posts.Count,
            _ => DefaultSortOptions.selector,
        };
    }

    protected override (Expression<Func<Topic, object>> selector, bool ascending) DefaultSortOptions
    {
        get
        {
            return (t => t.Name, true);
        }
    }
}
