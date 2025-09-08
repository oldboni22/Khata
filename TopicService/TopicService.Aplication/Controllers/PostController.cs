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
using Shared.Filters.Post;
using TopicService.API.Dto.Post;
using ILogger = Serilog.ILogger;

namespace TopicService.API.Controllers;

public class PostController(ITopicRepository repository, IUserGRpcClient userGRpcClient, IMapper mapper, ILogger logger) 
    : BaseController<Post,PostSortOptions>(repository, userGRpcClient, mapper, logger)
{
    
    [Authorize]
    [HttpPost("{topicId}/posts")]
    public async Task<PostReadDto> CreatePostAsync(
        PostCreateDto postCreateDto, Guid topicId, CancellationToken cancellationToken = default)
    {
        var topic = await Repository.FindByIdAsync(topicId, true, cancellationToken);
        
        if(topic is null)
        {
            throw new EntityNotFoundException<Topic>(topicId);
        }

        var senderId = User.GetAuth0Id();
        
        var senderUserId = await UserGRpcClient.FindUserIdByAuth0IdAsync(senderId!);
        
        var createdPost = topic!.AddPost(postCreateDto.Title, postCreateDto.Text, senderUserId);
        
        await Repository.UpdateAsync(cancellationToken);
        
        return Mapper.Map<PostReadDto>(createdPost);
    }

    [Authorize]
    [HttpDelete("{topicId}/posts/{postId}")]
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

    protected override (Expression<Func<Post, object>> selector, bool ascending) DefaultSortOptions => (post => post.Title, true);
}
