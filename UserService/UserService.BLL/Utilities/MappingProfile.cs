using AutoMapper;
using UserService.BLL.Models;
using UserService.BLL.Models.User;
using UserService.DAL.Models.Entities;

namespace UserService.BLL.Utilities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserModel>()
            .AfterMap((_, model) =>
            {
                foreach (var status in model.TopicStatuses)
                {
                    status.User = model;
                }
            })
            .ReverseMap();
        
        CreateMap<UserTopicRelation, UserTopicRelationModel>()
            .ForMember
                (
                    relation => relation.User,
                    opt => opt.Ignore()
                )
            .ReverseMap();

        CreateMap<UserCreateModel, UserModel>();
        
        CreateMap<UserUpdateModel, UserModel>();
    }
}
