using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Extensions;
using TopicService.API.Dto.Topic;
using TopicService.API.Utilities.LogMessages;

namespace TopicService.API;

[ApiController]
[Route("api/[controller]")]
public class TopicController(IApplicationService applicationService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<TopicReadDto> CreateTopicAsync([FromBody] TopicCreateDto topicCreateDto, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();
        
        return await applicationService.CreateHeadTopicAsync(senderId!, topicCreateDto, cancellationToken);
    }
    
    [Authorize]
    [HttpPost("{parentTopicId}")]
    public async Task<TopicReadDto> CreateSubTopicAsync(
        [FromBody] TopicCreateDto topicCreateDto, Guid parentTopicId, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();   
        
        return await applicationService.CreateSubTopicAsync(senderId!, topicCreateDto, parentTopicId, cancellationToken);
    }
}
