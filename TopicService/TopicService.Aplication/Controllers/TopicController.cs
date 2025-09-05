using System.Linq.Expressions;
using AutoMapper;
using Domain.Contracts.GRpc;
using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Filters.Topic;
using Shared.PagedList;
using TopicService.API.Dto.Post;
using TopicService.API.Dto.Topic;
using ILogger = Serilog.ILogger;

namespace TopicService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicController(ITopicRepository repository, IUserGRpcClient userGRpcClient, IMapper mapper, ILogger logger) : 
    BaseController<Topic, TopicSortOptions>(repository, userGRpcClient, mapper, logger)
{
    public async Task<TopicReadDto> CreateHeadTopicAsync(TopicCreateDto topicCreateDto, CancellationToken cancellationToken = default)
    {
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var topicEntity = Topic.Create(topicCreateDto.Name, senderUserId);
        
        var createdTopic = await Repository.CreateAsync(topicEntity, cancellationToken);
        
        return Mapper.Map<TopicReadDto>(createdTopic);
    }

    public async Task RemoveHeadTopicAsync(Guid topicId, CancellationToken cancellationToken = default)
    {
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var topic = await Repository.FindByIdAsync(topicId, cancellationToken: cancellationToken);

        if (topic is null)
        {
            throw new EntityNotFoundException<Topic>(topicId);
        }

        if (senderUserId != topic!.OwnerId)
        {
            throw new ForbiddenException();
        }
        
        await Repository.DeleteAsync(topicId, cancellationToken);
    }

    public async Task<TopicReadDto> CreateSubTopicAsync(
        TopicCreateDto topicCreateDto, Guid parentTopicId, CancellationToken cancellationToken = default)
    {
        var parentTopic = await Repository
            .FindByIdAsync(parentTopicId, true, cancellationToken);

        if (parentTopic is null)
        {
            throw new EntityNotFoundException<Topic>(parentTopicId);
        }

        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);

        if (await UserGRpcClient.HasStatusAsync(senderUserId, parentTopicId, UserTopicRelationStatus.Banned))
        {
            throw new ForbiddenException();
        }
        
        var createdTopic = parentTopic!.AddSubTopic(topicCreateDto.Name, senderUserId);

        await Repository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<TopicReadDto>(createdTopic);
    }

    public async Task RemoveSubTopicAsync(Guid parentTopicId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await Repository
            .FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            throw new EntityNotFoundException<Topic>(parentTopicId);
        }

        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        if (!await UserGRpcClient.HasStatusAsync(senderUserId, topicId, UserTopicRelationStatus.Moderator))
        {
            throw new ForbiddenException();
        }

        topic!.RemoveSubTopic(topicId, senderUserId);
        
        await Repository.UpdateAsync(cancellationToken);
    }

    public async Task<Topic> UpdateTopicOwnerAsync(Guid topicId, Guid userId, CancellationToken cancellationToken = default)
    {
        var topic = await Repository
            .FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            throw new EntityNotFoundException<Topic>(topicId);
        }
     
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);

        if (senderUserId == topic!.OwnerId)
        {
            throw new ForbiddenException();
        }
        
        topic!.SetOwner(userId);
        await Repository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<Topic>(topic);
    }


    public async Task<PostReadDto> CreatePostAsync(
        PostCreateDto postCreateDto, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await Repository.FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            throw new EntityNotFoundException<Topic>(topicId);
        }

        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId);
        
        var createdPost = topic!.AddPost(postCreateDto.Title, postCreateDto.Text, senderUserId);
        
        await Repository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<PostReadDto>(createdPost);
    }

    public async Task RemovePostAsync(Guid postId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await Repository.FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            throw new EntityNotFoundException<Topic>(topicId);
        }

        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        if (!await UserGRpcClient.HasStatusAsync(senderUserId, topicId, UserTopicRelationStatus.Moderator))
        {
            throw new ForbiddenException();
        }
        
        topic!.RemovePost(postId, senderUserId);
        
        await Repository.UpdateAsync(cancellationToken);
    }

    public async Task<PagedList<TopicReadDto>> FindHeadTopics(
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
        
        var selectors = ParseFilters(parameters.Filters);

        var topicEntities = await Repository
            .FindByConditionWithFilterAsync
            (
                expression,
                selectors,
                paginationParameters,
                false,
                cancellationToken
            );
        
        return Mapper.Map<PagedList<TopicReadDto>>(topicEntities);
    }

    public async Task<PagedList<TopicReadDto>> FindChildTopics(
        Guid parentTopicId,
        TopicSearchParameters parameters,
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        if (!await Repository.ExistsAsync(parentTopicId, cancellationToken))
        {
            throw new EntityNotFoundException<Topic>(parentTopicId);
        }
        
        Expression<Func<Topic, bool>> expression = t => t.ParentTopicId == parentTopicId;

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            expression = expression.And(t =>
                t.Name.Contains(parameters.SearchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
        
        var selectors = ParseFilters(parameters.Filters);
        
        var topicEntities = await Repository
            .FindByConditionWithFilterAsync
            (
                expression,
                selectors,
                paginationParameters,
                false,
                cancellationToken
            );
        
        return Mapper.Map<PagedList<TopicReadDto>>(topicEntities);
    }

    protected override Expression<Func<Topic, object>> ParseSortOption(TopicSortOptions sortOption)
    {
        return sortOption switch
        {
            TopicSortOptions.CreateDate => t => t.CreatedAt,
            TopicSortOptions.PostCount => t => t.Posts.Count,
            _ => DefaultSortOptions.selector,
        };
    }

    protected override (Expression<Func<Topic, object>> selector, bool ascending) DefaultSortOptions => (t => t.Name, true);
}
