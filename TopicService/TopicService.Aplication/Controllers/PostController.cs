using System.Linq.Expressions;
using AutoMapper;
using Domain.Contracts.GRpc;
using Domain.Contracts.MessageBroker;
using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Domain.Entities.Interactions;
using Domain.Exceptions;
using Messages.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinIoService;
using Shared.Enums;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.PagedList;
using Shared.Search.Post;
using TopicService.API.Dto.Post;
using ILogger = Serilog.ILogger;

namespace TopicService.API.Controllers;

[ApiController]
[Route("api/topics/{topicId}/posts")]
public class PostController(
    ITopicRepository topicRepository,
    IPostRepository postRepository,
    IUserGRpcClient userGRpcClient, 
    IMinioService minioService,
    IMessageSender messageSender,
    IMapper mapper, 
    ILogger logger) 
    : BaseController<Post, PostSortOptions>(topicRepository, userGRpcClient, mapper, logger)
{
    [Authorize]
    [HttpPost]
    public async Task<PostReadDto> CreatePostAsync(
        PostCreateDto postCreateDto, Guid topicId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var createdPost = topic.AddPost(postCreateDto.Title, postCreateDto.Text, senderUserId);
        
        await TopicRepository.UpdateAsync(cancellationToken);

        var userIds = await UserGRpcClient.FindUserIdsByTopicIdAsync(topicId, UserTopicRelationStatus.Subscribed);
        var moderatorIds = await UserGRpcClient.FindUserIdsByTopicIdAsync(topicId, UserTopicRelationStatus.Moderator);
        
        userIds.AddRange(moderatorIds);

        userIds.Add(topic.OwnerId);

        userIds.Remove(senderUserId);
        
        await messageSender.SendNotificationsCreateMessagesAsync(
            CreatePostNotifications(topicId, createdPost.Id, userIds), cancellationToken);
        
        return Mapper.Map<PostReadDto>(createdPost);
    }

    [Authorize]
    [HttpDelete("{postId}")]
    public async Task RemovePostAsync(Guid postId, Guid topicId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var isModerator = await UserGRpcClient.HasStatusAsync(senderUserId, topicId, UserTopicRelationStatus.Moderator);
        
        topic.RemovePost(postId, senderUserId, isModerator);
        
        await TopicRepository.UpdateAsync(cancellationToken);
    }
    
    [Authorize]
    [HttpDelete("{postId}/media")]
    public async Task RemovePostMediaAsync(Guid postId, Guid topicId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        if (senderUserId != post.AuthorId && senderUserId != topic.OwnerId &&
            await UserGRpcClient.HasStatusAsync(senderUserId, topicId, UserTopicRelationStatus.Moderator))
        {
            throw new ForbiddenException();
        }
        
        var minioKey = postId.ToString();

        await minioService.DeleteFileAsync(minioKey);
    }

    [Authorize]
    [HttpPut("{postId}")]
    public async Task<PostReadDto> UpdatePostAsync(
        PostUpdateDto dto ,Guid topicId ,Guid postId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        post.Update(senderUserId, dto.Title, dto.Text);
        
        await TopicRepository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<PostReadDto>(post);
    }
    
    [Authorize]
    [HttpPut("{postId}/media")]
    public async Task UpdatePostMediaAsync(
        IFormFile file ,Guid topicId ,Guid postId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);

        if (senderUserId != post.AuthorId)
        {
            throw new ForbiddenException();
        }

        var minioKey = postId.ToString();

        await minioService.UploadFileAsync(file, minioKey);
    }

    [HttpGet]
    public async Task<PagedList<PostReadDto>> FindPostsAsync(
        Guid topicId,
        [FromQuery] PostSearchOptions searchOptions,
        [FromQuery] PaginationParameters? paginationParameters,
        CancellationToken cancellationToken = default)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        paginationParameters ??= new PaginationParameters();
        
        var selectors = ParseSortOptions(searchOptions.SortEntries);

        var pagedPosts = await postRepository
            .FindByConditionAsync
            (
                topicId,
                paginationParameters,
                searchOptions.Filter,
                selectors,
                false,
                cancellationToken
            );
        
        return Mapper.Map<PagedList<PostReadDto>>(pagedPosts);
    }
    
    [HttpGet("{postId}")]
    public async Task<PostReadDto> FindPostAsync(Guid topicId, Guid postId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        return Mapper.Map<PostReadDto>(post);
    }
    
    [Authorize]
    [HttpGet("{postId}/media")]
    public async Task<FileResult> GetPostMediaAsync(
        Guid postId, Guid topicId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var minioKey = postId.ToString();

        var (stream, stats) = await minioService.GetFileAsync(minioKey) 
                              ?? throw new Exception();

        return File(stream, stats.ContentType, stats.ObjectName);
    }

    [Authorize]
    [HttpPost("{postId}/interactions")]
    public async Task AddInteractionAsync(
        [FromBody] InteractionType interactionType, Guid topicId, Guid postId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var senderId = User.GetAuth0Id();
        
        var userId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        post.AddInteraction(userId, interactionType);
        
        await TopicRepository.UpdateAsync(cancellationToken);
    }
    
    [Authorize]
    [HttpDelete("{postId}/interactions/{interactionId}")]
    public async Task RemoveInteractionAsync(
        Guid topicId, Guid postId, Guid interactionId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var senderId = User.GetAuth0Id();
        
        var userId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        post.RemoveInteraction(interactionId, userId);
        
        await TopicRepository.UpdateAsync(cancellationToken);
    }
    
    [Authorize]
    [HttpPut("{postId}/interactions/{interactionId}")]
    public async Task UpdateInteractionAsync(
        [FromBody] InteractionType interactionType, 
        Guid topicId, 
        Guid postId, 
        Guid interactionId,
        CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var interaction = post.Interactions.SingleOrDefault(i => i.Id == interactionId);

        if (interaction is null)
        {
            throw new EntityNotFoundException<PostInteraction>(interactionId);
        }
        
        var senderId = User.GetAuth0Id();
        
        var userId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        interaction.SetRating(interactionType, userId);
        
        await TopicRepository.UpdateAsync(cancellationToken);
    }
    
    protected override Expression<Func<Post, object>> ParseSortOption(PostSortOptions sortOption)
    {
        return sortOption switch
        {
            PostSortOptions.Title => post => post.Title,
            PostSortOptions.CreateDate => post => post.CreatedAt,
            PostSortOptions.LikeCount => post => post.LikeCount,
            PostSortOptions.DislikeCount => post => post.DislikeCount,
            _ => DefaultSortOptions.selector
        };
    }

    protected override (Expression<Func<Post, object>> selector, bool ascending) DefaultSortOptions =>
        (post => post.Title, true);

    private List<Notification> CreatePostNotifications(Guid topicId, Guid postId, IEnumerable<Guid> userIds)
    {
        return userIds.Select(uid => 
            new Notification
            {
                UserId = uid,
                EntityType = EntityType.Post,
                EntityId = postId,
                Parent = new ParentEntity
                {
                    Id = topicId,
                    Type = EntityType.Topic
                }
            }
        ).ToList();
    }
}
