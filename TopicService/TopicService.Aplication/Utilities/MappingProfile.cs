using AutoMapper;
using Domain.Entities;
using TopicService.API.Dto.Post;
using TopicService.API.Dto.Topic;

namespace TopicService.API.Utilities;

public class MappingProfile : Profile
{
    protected MappingProfile()
    {
        CreateMap<TopicCreateDto, Topic>();

        CreateMap<Topic, TopicReadDto>();
        
        CreateMap<PostCreateDto, Post>();
        
        CreateMap<Post, PostReadDto>();
    }
}
