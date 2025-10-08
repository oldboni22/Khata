using AutoMapper;
using Shared.PagedList;
using UserService.API.DTO;
using UserService.API.Utilities.Validation;
using UserService.BLL.Models;
using UserService.BLL.Models.User;

namespace UserService.API.Utilities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserCreateDto, UserCreateModel>();

        CreateMap<UserTopicRelationModel, UserTopicRelationReadDto>()
            .PreserveReferences();
        
        CreateMap<UserUpdateDto, UserUpdateModel>();

        CreateMap<UserModel, UserReadDto>()
            .PreserveReferences();
    }
}
