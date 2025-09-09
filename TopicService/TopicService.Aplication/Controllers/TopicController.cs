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
[Route("api/topics")]
public class TopicController(
    ITopicRepository repository, IUserGRpcClient userGRpcClient, IMapper mapper, ILogger logger) : 
    BaseController<Topic, TopicSortOptions>(repository, userGRpcClient, mapper, logger)
{
    [Authorize]
    [HttpPost("create")]
    public async Task<TopicReadDto> CreateParentTopicAsync(
        TopicCreateDto topicCreateDto, CancellationToken cancellationToken = default)
    {
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var topicEntity = Topic.Create(topicCreateDto.Name, senderUserId);
        
        var createdTopic = await Repository.CreateAsync(topicEntity, cancellationToken);

        await Repository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<TopicReadDto>(createdTopic);
    }

    [Authorize]
    [HttpDelete("{parentTopicId}")]
    public async Task RemoveParentTopicAsync(Guid parentTopicId, CancellationToken cancellationToken = default)
    {
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var topic = await Repository.FindByIdAsync(parentTopicId, cancellationToken: cancellationToken);

        if (topic is null)
        {
            throw new EntityNotFoundException<Topic>(parentTopicId);
        }

        if (senderUserId != topic!.OwnerId)
        {
            throw new ForbiddenException();
        }
        
        await Repository.DeleteAsync(parentTopicId, cancellationToken);
        
        await Repository.UpdateAsync(cancellationToken);
    }

    [Authorize]
    [HttpPost("{parentTopicId}/subtopics")]
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

    [Authorize]
    [HttpDelete("{parentTopicId}/subtopics/{topicId}")]
    public async Task RemoveSubTopicAsync(
        Guid parentTopicId, Guid topicId, CancellationToken cancellationToken = default)
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

    [Authorize]
    [HttpPatch("{topicId}/owner")]
    public async Task<Topic> UpdateTopicOwnerAsync(
        Guid topicId, OwnershipTransferDto dto, CancellationToken cancellationToken = default)
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
        
        topic!.SetOwner(dto.NewOwnerId);
        
        await Repository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<Topic>(topic);
    }
    
    [HttpGet("parentTopics")]
    public async Task<PagedList<TopicReadDto>> FindParentTopics(
        [FromQuery] TopicSearchParameters parameters, 
        [FromQuery] PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Topic, bool>> predicate = t => t.ParentTopicId == null;

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            predicate = predicate.And(t =>
                t.Name.Contains(parameters.SearchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
        
        var selectors = ParseFilters(parameters.Filters);

        var topicEntities = await Repository
            .FindByConditionAsync
            (
                predicate,
                paginationParameters,
                selectors,
                false,
                cancellationToken
            );
        
        return Mapper.Map<PagedList<TopicReadDto>>(topicEntities);
    }

    [HttpGet("{parentTopicId}/subtopics")]
    public async Task<PagedList<TopicReadDto>> FindChildTopics(
        Guid parentTopicId,
        [FromQuery] TopicSearchParameters parameters,
        [FromQuery] PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        if (await Repository.FindByIdAsync(parentTopicId, false, cancellationToken) is null)
        {
            throw new EntityNotFoundException<Topic>(parentTopicId);
        }
        
        Expression<Func<Topic, bool>> predicate = t => t.ParentTopicId == parentTopicId;

        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            predicate = predicate.And(t =>
                t.Name.Contains(parameters.SearchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
        
        var selectors = ParseFilters(parameters.Filters);
        
        var topicEntities = await Repository
            .FindByConditionAsync
            (
                predicate,
                paginationParameters,
                selectors,
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

    protected override (Expression<Func<Topic, object>> selector, bool ascending) DefaultSortOptions => 
        (t => t.Name, true);
}
