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
            .ReverseMap();
        
        CreateMap<UserTopicRelation, UserTopicRelationModel>()
            .ReverseMap();

        CreateMap<UserCreateModel, UserModel>();
        CreateMap<UserUpdateModel, UserModel>();
    }
}
