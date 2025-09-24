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
using MinIoService;
using Shared.Enums;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Filters.Comment;
using Shared.PagedList;
using TopicService.API.Dto.Comment;
using ILogger = Serilog.ILogger;

namespace TopicService.API.Controllers;

[ApiController]
[Route("api/topics/{topicId}/posts/{postId}/comments")]
public class CommentController(
    ITopicRepository topicRepository,
    IGenericReadOnlyRepository<Comment> commentRepository,
    IUserGRpcClient userGRpcClient,
    IMinioService minioService,
    IMessageSender messageSender,
    IMapper mapper,
    ILogger logger) : BaseController<Comment, CommentSortOptions>(topicRepository, userGRpcClient, mapper, logger)
{
    [HttpPost]
    [Authorize]
    public async Task<CommentReadDto> CreateCommentAsync(
        Guid topicId, Guid postId, [FromBody] string text, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);
        
        var post = topic.Posts.SingleOrDefault(p => p.Id == postId) 
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var createdComment = post.AddComment(text, senderUserId);

        await TopicRepository.UpdateAsync(cancellationToken);
        
        await messageSender.SendNotificationsCreateMessagesAsync(
            CreateCommentNotification(topicId, postId, createdComment.Id, post.AuthorId), cancellationToken);
        
        return Mapper.Map<CommentReadDto>(createdComment);
    }
    
    [Authorize]
    [HttpDelete("{commentId}")]
    public async Task DeleteCommentAsync(
        Guid topicId, Guid postId, Guid commentId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAndThenCommentsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        post.RemoveComment(commentId, senderUserId);

        await TopicRepository.UpdateAsync(cancellationToken);
    }
    
    [Authorize]
    [HttpDelete("{commentId}/media")]
    public async Task RemoveCommentMediaAsync(
        Guid postId, Guid topicId, Guid commentId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAndThenCommentsAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var comment = post.Comments.SingleOrDefault(comm => comm.Id == commentId) 
                      ?? throw new EntityNotFoundException<Comment>(commentId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        if (senderUserId != comment.UserId && senderUserId != topic.OwnerId &&
            await UserGRpcClient.HasStatusAsync(senderUserId, topicId, UserTopicRelationStatus.Moderator))
        {
            throw new ForbiddenException();
        }
        
        var minioKey = commentId.ToString();

        await minioService.DeleteFileAsync(minioKey);
    }
    
    [Authorize]
    [HttpPut("{commentId}")]
    public async Task<CommentReadDto> UpdateCommentAsync(
        Guid topicId, Guid postId, Guid commentId, [FromBody] string text, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAndThenCommentsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var comment = post.Comments.SingleOrDefault(comm => comm.Id == commentId) 
                      ?? throw new EntityNotFoundException<Comment>(commentId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        comment.SetText(text, senderUserId);

        await TopicRepository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<CommentReadDto>(comment);
    }

    [Authorize]
    [HttpPut("{commentId}/media")]
    public async Task UpdateCommentMediaAsync(
        IFormFile file, Guid postId, Guid topicId, Guid commentId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAndThenCommentsAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var comment = post.Comments.SingleOrDefault(comm => comm.Id == commentId) 
                      ?? throw new EntityNotFoundException<Comment>(commentId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        if (senderUserId != comment.UserId && senderUserId != topic.OwnerId &&
            await UserGRpcClient.HasStatusAsync(senderUserId, topicId, UserTopicRelationStatus.Moderator))
        {
            throw new ForbiddenException();
        }
        
        var minioKey = commentId.ToString();

        await minioService.UploadFileAsync(file, minioKey);
    }
    
    [HttpGet("{commentId}")]
    public async Task<CommentReadDto> FindCommentAsync(
        Guid topicId, Guid postId, Guid commentId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAndThenCommentsAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var comment = post.Comments.SingleOrDefault(comm => comm.Id == commentId) 
                      ?? throw new EntityNotFoundException<Comment>(commentId);
        
        return Mapper.Map<CommentReadDto>(comment);
    }

    [Authorize]
    [HttpGet("{commentId}/media")]
    public async Task<FileResult> FindTopicWithPostsAndThenCommentsAsync(
        Guid postId, Guid topicId, Guid commentId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var comment = post.Comments.SingleOrDefault(comm => comm.Id == commentId) 
                      ?? throw new EntityNotFoundException<Comment>(commentId);
        
        var minioKey = commentId.ToString();

        var (stream, stats) = await minioService.GetFileAsync(minioKey) 
                              ?? throw new Exception();

        return File(stream, stats.ContentType, stats.ObjectName);
    }
    
    [HttpGet]
    public async Task<PagedList<CommentReadDto>> FindCommentsAsync(
        Guid topicId,
        Guid postId,
        [FromQuery] CommentSearchOptions? searchOptions,
        [FromQuery] PaginationParameters? paginationParameters,
        CancellationToken cancellationToken = default)
    {
        var topic = await TopicRepository.FindTopicWithPostsAndThenCommentsAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        CheckQueryParameters(ref paginationParameters);
        
        Expression<Func<Comment, bool>> predicate = comm => comm.PostId == postId;
        
        if (!string.IsNullOrEmpty(searchOptions?.SearchTerm))
        {
            predicate = predicate.And(s => 
                s.Text.Contains(searchOptions.SearchTerm, StringComparison.CurrentCultureIgnoreCase));
        }

        var selectors = ParseFilters(searchOptions?.Filters);
        
        var pagedComments = commentRepository.FindByConditionAsync
        (
            predicate,
            paginationParameters,
            selectors,
            false,
            cancellationToken
        );
        
        return Mapper.Map<PagedList<CommentReadDto>>(pagedComments);
    }
    
    [Authorize]
    [HttpPost("{commentId}/interactions")]
    public async Task AddInteractionAsync(
        [FromBody] InteractionType interactionType, 
        Guid topicId, 
        Guid postId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAndThenCommentsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var comment = post.Comments.SingleOrDefault(comm => comm.Id == commentId) 
                      ?? throw new EntityNotFoundException<Comment>(commentId);
        
        var senderId = User.GetAuth0Id();
        
        var userId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        comment.AddInteraction(userId, interactionType);
        
        await TopicRepository.UpdateAsync(cancellationToken);
    }
    
    [Authorize]
    [HttpDelete("{commentId}/interactions/{interactionId}")]
    public async Task RemoveInteractionAsync(
        Guid topicId,
        Guid postId,
        Guid commentId,
        Guid interactionId,
        CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAndThenCommentsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var comment = post.Comments.SingleOrDefault(comm => comm.Id == commentId) 
                      ?? throw new EntityNotFoundException<Comment>(commentId);
        
        var senderId = User.GetAuth0Id();
        
        var userId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        comment.RemoveInteraction(interactionId, userId);
        
        await TopicRepository.UpdateAsync(cancellationToken);
    }
    
    [Authorize]
    [HttpPut("{commentId}/interactions/{interactionId}")]
    public async Task UpdateInteractionAsync(
        [FromBody] InteractionType interactionType, 
        Guid topicId, 
        Guid postId, 
        Guid commentId,
        Guid interactionId,
        CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindTopicWithPostsAndThenCommentsAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var comment = post.Comments.SingleOrDefault(comm => comm.Id == commentId) 
                      ?? throw new EntityNotFoundException<Comment>(commentId);
        
        var interaction = comment.Interactions.SingleOrDefault(i => i.Id == interactionId);

        if (interaction is null)
        {
            throw new EntityNotFoundException<PostInteraction>(interactionId);
        }
        
        var senderId = User.GetAuth0Id();
        
        var userId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        interaction.SetRating(interactionType, userId);
        
        await TopicRepository.UpdateAsync(cancellationToken);
    }
    
    protected override Expression<Func<Comment, object>> ParseSortOption(CommentSortOptions sortOption)
    {
        return sortOption switch
        {
            CommentSortOptions.CreateDate => comm => comm.CreatedAt,
            CommentSortOptions.LikeCount => comm => comm.LikeCount,
            CommentSortOptions.DislikeCount => comm => comm.DislikeCount,
            _ => DefaultSortOptions.selector,
        };
    }

    protected override (Expression<Func<Comment, object>> selector, bool ascending) DefaultSortOptions =>
        (comm => comm.CreatedAt, true);

    private List<Notification> CreateCommentNotification(Guid postId, Guid topicId, Guid commentId, Guid userId)
    {
        return
        [
            new Notification
            {
                UserId = userId,
                EntityType = EntityType.Comment,
                EntityId = commentId,
                Parent = new ParentEntity
                {
                    Type = EntityType.Post,
                    Id = postId,
                    Parent = new ParentEntity
                    {
                        Type = EntityType.Topic,
                        Id = topicId
                    }
                }
            }
        ];
    }
}
