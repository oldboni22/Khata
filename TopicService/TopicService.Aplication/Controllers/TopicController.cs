using System.Linq.Expressions;
using AutoMapper;
using Domain.Contracts.GRpc;
using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.PagedList;
using Shared.Search.Topic;
using TopicService.API.Dto.Post;
using TopicService.API.Dto.Topic;
using ILogger = Serilog.ILogger;

namespace TopicService.API.Controllers;

[ApiController]
[Route("api/topics")]
public class TopicController(
    ITopicRepository topicRepository, IUserGRpcClient userGRpcClient, IMapper mapper, ILogger logger) : 
    BaseController<Topic, TopicSortOptions>(topicRepository, userGRpcClient, mapper, logger)
{
    [Authorize]
    [HttpPost]
    public async Task<TopicReadDto> CreateParentTopicAsync(
        TopicCreateDto topicCreateDto, CancellationToken cancellationToken = default)
    {
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var topicEntity = Topic.Create(topicCreateDto.Name, senderUserId);
        
        var createdTopic = await TopicRepository.CreateAsync(topicEntity, cancellationToken);

        await TopicRepository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<TopicReadDto>(createdTopic);
    }

    [Authorize]
    [HttpDelete("{parentTopicId}")]
    public async Task RemoveParentTopicAsync(Guid parentTopicId, CancellationToken cancellationToken = default)
    {
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var topic = await TopicRepository.FindByIdAsync(parentTopicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(parentTopicId);

        if (senderUserId != topic!.OwnerId)
        {
            throw new ForbiddenException();
        }
        
        await TopicRepository.DeleteAsync(parentTopicId, cancellationToken);
        
        await TopicRepository.UpdateAsync(cancellationToken);
    }

    [Authorize]
    [HttpPost("{parentTopicId}/subtopics")]
    public async Task<TopicReadDto> CreateSubTopicAsync(
        TopicCreateDto topicCreateDto, Guid parentTopicId, CancellationToken cancellationToken = default)
    {
        var parentTopic = await TopicRepository.FindByIdAsync(parentTopicId, true, cancellationToken) 
                          ?? throw new EntityNotFoundException<Topic>(parentTopicId);

        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);

        if (await UserGRpcClient.HasStatusAsync(senderUserId, parentTopicId, UserTopicRelationStatus.Banned))
        {
            throw new ForbiddenException();
        }
        
        var createdTopic = parentTopic!.AddSubTopic(topicCreateDto.Name, senderUserId);

        await TopicRepository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<TopicReadDto>(createdTopic);
    }

    [Authorize]
    [HttpDelete("{parentTopicId}/subtopics/{topicId}")]
    public async Task RemoveSubTopicAsync(
        Guid parentTopicId, Guid topicId, CancellationToken cancellationToken = default)
    {
        var parentTopic = await TopicRepository.FindTopicWithSubTopicsAsync(parentTopicId, true, cancellationToken) 
                          ?? throw new EntityNotFoundException<Topic>(parentTopicId);

        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        if (!await UserGRpcClient.HasStatusAsync(senderUserId, topicId, UserTopicRelationStatus.Moderator))
        {
            throw new ForbiddenException();
        }

        parentTopic!.RemoveSubTopic(topicId, senderUserId);
        
        await TopicRepository.UpdateAsync(cancellationToken);
    }

    [Authorize]
    [HttpPatch("{topicId}/owner")]
    public async Task<Topic> UpdateTopicOwnerAsync(
        Guid topicId, OwnershipTransferDto dto, CancellationToken cancellationToken = default)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, true, cancellationToken)
                    ?? throw new EntityNotFoundException<Topic>(topicId);
     
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);

        if (senderUserId != topic!.OwnerId)
        {
            throw new ForbiddenException();
        }
        
        topic!.SetOwner(dto.NewOwnerId);
        
        await TopicRepository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<Topic>(topic);
    }
    
    [HttpGet("parentTopics")]
    public async Task<PagedList<TopicReadDto>> FindParentTopics(
        [FromQuery] TopicSearchParameters searchOptions, 
        [FromQuery] PaginationParameters? paginationParameters,
        CancellationToken cancellationToken = default)
    {
        paginationParameters ??= new();
        
        var selectors = ParseSortOptions(searchOptions.SortEntries);

        var topicEntities = await TopicRepository
            .FindByConditionAsync
            (
                paginationParameters,
                null,
                searchOptions.Filter,
                selectors,
                false,
                cancellationToken
            );
        
        return Mapper.Map<PagedList<TopicReadDto>>(topicEntities);
    }

    [HttpGet("{parentTopicId}/subtopics")]
    public async Task<PagedList<TopicReadDto>> FindChildTopics(
        Guid parentTopicId,
        [FromQuery] TopicSearchParameters searchOptions,
        [FromQuery] PaginationParameters? paginationParameters,
        CancellationToken cancellationToken = default)
    {
        if (await TopicRepository.FindByIdAsync(parentTopicId, false, cancellationToken) is null)
        {
            throw new EntityNotFoundException<Topic>(parentTopicId);
        }

        paginationParameters ??= new();
        
        var selectors = ParseSortOptions(searchOptions.SortEntries);
        
        var topicEntities = await TopicRepository
            .FindByConditionAsync
            (
                paginationParameters,
                parentTopicId,
                searchOptions.Filter,
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
