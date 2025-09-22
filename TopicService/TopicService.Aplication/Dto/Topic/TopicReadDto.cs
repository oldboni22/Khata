using AutoMapper;
using Domain.Entities;

namespace TopicService.API.Dto.Topic;

public class TopicReadDto
{
    public Guid Id { get; set; }
    
    public Guid? ParentTopicId { get; set; }
    
    public Guid OwnerId { get; set; }
    
    public required string Name { get; set; }
}
