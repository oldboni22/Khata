using AutoMapper;
using Domain.Entities;
using TopicService.API.Dto.Post;
using TopicService.API.Dto.Topic;
using TopicReadDto = Domain.Entities.TopicReadDto;

namespace TopicService.API.Utilities;

public class MappingProfile : Profile
{
    protected MappingProfile()
    {
        CreateMap<TopicReadDto, Dto.Topic.TopicReadDto>();
        
        CreateMap<Post, PostReadDto>();
    }
}
