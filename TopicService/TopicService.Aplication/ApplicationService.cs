using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.RepositoryContracts;
using Infrastructure.gRpc;
using Shared.Exceptions;
using TopicService.API.Dto.Post;
using TopicService.API.Dto.Topic;
using TopicService.API.Utilities.LogMessages;

namespace TopicService.API;

public interface IApplicationService
{
    Task<TopicReadDto> CreateHeadTopicAsync(string senderId, TopicCreateDto topicCreateDto, CancellationToken cancellationToken = default);

    Task RemoveHeadTopicAsync(string senderId, Guid topicId, CancellationToken cancellationToken = default);

    Task<TopicReadDto> CreateSubTopicAsync(
       string senderId, TopicCreateDto topicCreateDto, Guid parentTopicId, CancellationToken cancellationToken = default);
    
    Task RemoveSubTopicAsync(string senderId, Guid parentTopicId, Guid topicId, CancellationToken cancellationToken = default); 
    
    Task<PostReadDto> CreatePostAsync(string senderId, PostCreateDto postCreateDto, Guid topicId, CancellationToken cancellationToken = default);
    
    Task RemovePostAsync(string senderId, Guid postId, Guid topicId, CancellationToken cancellationToken = default);
}

public class ApplicationService(
    ITopicRepository topicRepository, IUserGRpcClient userGRpcClient, IMapper mapper, Serilog.ILogger logger) : IApplicationService
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
        var parentTopic = await topicRepository.FindByIdAsync(parentTopicId, cancellationToken: cancellationToken);

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
        var topic = await topicRepository.FindByIdAsync(topicId, cancellationToken: cancellationToken);


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
        var topic = await topicRepository.FindByIdAsync(topicId, cancellationToken: cancellationToken);
        
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
        var topic = await topicRepository.FindByIdAsync(topicId, cancellationToken: cancellationToken);
        
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

    private void ThrowNotFoundException<T>(Guid id) where T : EntityBase
    {
        logger.Information(EntityNotFoundLogMessage<T>.Generate(id));
        
        throw new EntityNotFoundException<T>(id);
    }  
}
