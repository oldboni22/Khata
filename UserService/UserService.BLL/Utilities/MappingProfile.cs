using AutoMapper;
using Shared.PagedList;
using UserService.BLL.Models;
using UserService.BLL.Models.User;
using UserService.DAL.Models.Entities;

namespace UserService.BLL.Utilities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserModel>()
            .PreserveReferences()
            .ReverseMap();

        CreateMap<UserTopicRelation, UserTopicRelationModel>()
            .PreserveReferences()
            .ReverseMap();

        CreateMap<UserCreateModel, User>();
        
        CreateMap<UserUpdateModel, User>();
    }
}
