using AutoMapper;
using Domain.Entities;
using TopicService.API.Dto.Post;
using TopicService.API.Dto.Topic;

namespace TopicService.API.Utilities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Topic, Dto.Topic.TopicReadDto>();
        
        CreateMap<Post, PostReadDto>();
    }
}
