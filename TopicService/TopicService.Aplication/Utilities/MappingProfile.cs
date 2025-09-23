using AutoMapper;
using Domain.Entities;
using Shared.PagedList;
using TopicService.API.Dto.Comment;
using TopicService.API.Dto.Post;
using TopicService.API.Dto.Topic;

namespace TopicService.API.Utilities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Topic, Dto.Topic.TopicReadDto>();

        CreateMap<PagedList<Topic>, PagedList<TopicReadDto>>();
        
        CreateMap<Post, PostReadDto>();

        CreateMap<PagedList<Post>, PagedList<PostReadDto>>();
        
        CreateMap<Comment, CommentReadDto>();
        
        CreateMap<PagedList<Comment>, PagedList<CommentReadDto>>();
    }
}
