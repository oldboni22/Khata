using System.Linq.Expressions;
using AutoMapper;
using Domain.Contracts.GRpc;
using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Domain.Entities.Interactions;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Extensions;
using Shared.Filters.Comment;
using Shared.PagedList;
using TopicService.API.Dto.Comment;
using ILogger = Serilog.ILogger;

namespace TopicService.API.Controllers;

[ApiController]
[Route("api/topics/{topicId}/posts/{postId}/comments")]
public class CommentController(
    IGenericRepository<Topic> postTopicRepository,
    IGenericReadOnlyRepository<Comment> commentRepository,
    IUserGRpcClient userGRpcClient,
    IMapper mapper,
    ILogger logger) : BaseController<Comment, CommentSortOptions>(postTopicRepository, userGRpcClient, mapper, logger)
{
    [HttpPost]
    [Authorize]
    public async Task<CommentReadDto> CreateCommentAsync(
        Guid topicId, Guid postId, [FromBody] string text, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId) 
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var comment = post.AddComment(text, senderUserId);

        await TopicRepository.UpdateAsync(cancellationToken);
        
        return mapper.Map<CommentReadDto>(comment);
    }
    
    [Authorize]
    [HttpDelete("{commentId}")]
    public async Task DeleteCommentAsync(
        Guid topicId, Guid postId, Guid commentId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        post.RemoveComment(commentId, senderUserId);

        await TopicRepository.UpdateAsync(cancellationToken);
    }
    
    [Authorize]
    [HttpPut("{commentId}")]
    public async Task<CommentReadDto> UpdateCommentAsync(
        Guid topicId, Guid postId, Guid commentId, [FromBody] string text, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, true, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var comment = post.Comments.SingleOrDefault(comm => comm.Id == commentId) 
                      ?? throw new EntityNotFoundException<Comment>(commentId);
        
        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        comment.SetText(text, senderUserId);

        await TopicRepository.UpdateAsync(cancellationToken);
        
        return mapper.Map<CommentReadDto>(comment);
    }

    [HttpGet("{commentId}")]
    public async Task<CommentReadDto> FindCommentAsync(
        Guid topicId, Guid postId, Guid commentId, CancellationToken cancellationToken)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        var comment = post.Comments.SingleOrDefault(comm => comm.Id == commentId) 
                      ?? throw new EntityNotFoundException<Comment>(commentId);
        
        return mapper.Map<CommentReadDto>(comment);
    }

    [HttpGet]
    public async Task<PagedList<CommentReadDto>> FindCommentsAsync(
        Guid topicId,
        Guid postId,
        [FromQuery] CommentSearchParameters searchParameters,
        [FromQuery] PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        var topic = await TopicRepository.FindByIdAsync(topicId, false, cancellationToken) 
                    ?? throw new EntityNotFoundException<Topic>(topicId);

        var post = topic.Posts.SingleOrDefault(p => p.Id == postId)
                   ?? throw new EntityNotFoundException<Post>(postId);
        
        Expression<Func<Comment, bool>> predicate = comm => comm.PostId == postId;

        if (!string.IsNullOrEmpty(searchParameters.SearchTerm))
        {
            predicate = predicate.And(s => 
                s.Text.Contains(searchParameters.SearchTerm, StringComparison.CurrentCultureIgnoreCase));
        }

        var selectors = ParseFilters(searchParameters.Filters);
        
        var pagedComments = commentRepository.FindByConditionAsync
        (
            predicate,
            paginationParameters,
            selectors,
            false,
            cancellationToken
        );
        
        return mapper.Map<PagedList<CommentReadDto>>(pagedComments);
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
        var topic = await TopicRepository.FindByIdAsync(topicId, true, cancellationToken) 
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
        var topic = await TopicRepository.FindByIdAsync(topicId, true, cancellationToken) 
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
        var topic = await TopicRepository.FindByIdAsync(topicId, true, cancellationToken) 
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
}
