using AutoMapper;
using Domain.Entities;

namespace TopicService.API.Dto.Topic;

public class TopicReadDto
{
    public required string Name { get; set; }
}
