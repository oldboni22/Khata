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

namespace TopicService.API;

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
        TopicSearchFilter filter, 
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default);
    
    Task<PagedList<Topic>> FindChildTopics(
        Guid parentTopicId,
        TopicSearchFilter filter, 
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default);
}

public class TopicTopicApplicationService(
    ITopicRepository topicRepository, IUserGRpcClient userGRpcClient, IMapper mapper, Serilog.ILogger logger) : ITopicApplicationService
{
    public async Task<TopicReadDto> CreateHeadTopicAsync(string senderId, TopicCreateDto topicCreateDto, CancellationToken cancellationToken = default)
    {
        var senderUserId = await userGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var topicEntity = Topic.Create(topicCreateDto.Name, senderUserId);
        
        var createdTopic = await topicRepository.CreateAsync(topicEntity, cancellationToken);
        
        return mapper.Map<TopicReadDto>(createdTopic);
    }

    public async Task RemoveHeadTopicAsync(string senderId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var senderUserId = await userGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var topic = await topicRepository.FindByIdAsync(topicId, cancellationToken: cancellationToken);

        if (topic is null)
        {
            ThrowNotFoundException<Topic>(topicId);
        }

        if (senderUserId != topic!.OwnerId)
        {
            throw new ForbiddenException();
        }
        
        await topicRepository.DeleteAsync(topicId, cancellationToken);
    }

    public async Task<TopicReadDto> CreateSubTopicAsync(string senderId, TopicCreateDto topicCreateDto, Guid parentTopicId, CancellationToken cancellationToken = default)
    {
        var parentTopic = await topicRepository
            .FindByIdAsync(parentTopicId, true, cancellationToken);

        if (parentTopic is null)
        {
            ThrowNotFoundException<Topic>(parentTopicId);    
        }

        var senderUserId = await userGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var createdTopic = parentTopic!.AddSubTopic(topicCreateDto.Name, senderUserId);

        await topicRepository.SaveChangesAsync(cancellationToken);
        
        return mapper.Map<TopicReadDto>(createdTopic);
    }

    public async Task RemoveSubTopicAsync(
        string senderId, Guid parentTopicId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await topicRepository
            .FindByIdAsync(topicId, true, cancellationToken);


        if(topic is null)
        {
            ThrowNotFoundException<Topic>(topicId);
        }

        var senderUserId = await userGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        if (!await userGRpcClient.IsModeratorAsync(senderUserId, topicId))
        {
            throw new ForbiddenException();
        }

        topic!.RemoveSubTopic(topicId, senderUserId);
        
        await topicRepository.SaveChangesAsync(cancellationToken);
    }


    public async Task<PostReadDto> CreatePostAsync(
        string senderId,PostCreateDto postCreateDto, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await topicRepository.FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            ThrowNotFoundException<Topic>(topicId);
        }

        var senderUserId = await userGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var createdPost = topic!.AddPost(postCreateDto.Title, postCreateDto.Text, senderUserId);
        
        await topicRepository.SaveChangesAsync(cancellationToken);
        
        return mapper.Map<PostReadDto>(createdPost);
    }

    public async Task RemovePostAsync(string senderId, Guid postId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await topicRepository.FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            ThrowNotFoundException<Topic>(topicId);
        }

        var senderUserId = await userGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        if (!await userGRpcClient.IsModeratorAsync(senderUserId, topicId))
        {
            throw new ForbiddenException();
        }
        
        topic!.RemovePost(postId, senderUserId);
        
        await topicRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedList<Topic>> FindHeadTopics(
        TopicSearchFilter filter, 
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Topic, bool>> expression = t => t.ParentTopicId == null;

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            expression = expression.And(t =>
                t.Name.Contains(filter.SearchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
        
        return await topicRepository
            .FindByConditionAsync
            (
                expression,
                filter.SortOptions,
                filter.Ascending,
                paginationParameters,
                false,
                cancellationToken
            );
    }

    public async Task<PagedList<Topic>> FindChildTopics(
        Guid parentTopicId,
        TopicSearchFilter filter,
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        if (!await topicRepository.ExistsAsync(parentTopicId, cancellationToken))
        {
            throw new EntityNotFoundException<Topic>(parentTopicId);
        }
        
        Expression<Func<Topic, bool>> expression = t => t.ParentTopicId == parentTopicId;

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            expression = expression.And(t =>
                t.Name.Contains(filter.SearchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
        
        return await topicRepository
            .FindByConditionAsync
            (
                expression,
                filter.SortOptions,
                filter.Ascending,
                paginationParameters,
                false,
                cancellationToken
            );
    }


    private void ThrowNotFoundException<T>(Guid id) where T : EntityBase
    {
        logger.Information(EntityNotFoundLogMessage<T>.Generate(id));
        
        throw new EntityNotFoundException<T>(id);
    }  
}
