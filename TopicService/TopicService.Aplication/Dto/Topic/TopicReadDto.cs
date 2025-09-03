using AutoMapper;
using Domain.Entities;

namespace TopicService.API.Dto.Topic;

public class TopicReadDto
{
    public required string Name { get; set; }

    public List<Domain.Entities.Topic> SubTopics { get; set; } = [];

    public List<Domain.Entities.Post> Posts { get; set; } = [];
}
