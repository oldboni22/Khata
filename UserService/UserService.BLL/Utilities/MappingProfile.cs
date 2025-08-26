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
            .PreserveReferences();

        CreateMap<UserTopicRelation, UserTopicRelationModel>()
            .PreserveReferences();

        CreateMap<UserCreateModel, UserModel>();
        
        CreateMap<UserUpdateModel, UserModel>();
    }
}
