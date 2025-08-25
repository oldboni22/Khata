using AutoMapper;
using UserService.API.DTO;
using UserService.API.Utilities.Validation;
using UserService.BLL.Models;
using UserService.BLL.Models.User;

namespace UserService.API.Utilities;

public class MappingProfile : Profile
{
    protected MappingProfile()
    {
        CreateMap<UserCreateDto, UserCreateModel>();

        CreateMap<UserTopicRelationModel, UserTopicRelationDto>()
            .ForMember
            (
                model => model.User,
                opt => opt.Ignore()
            )
            .ReverseMap();
        
        CreateMap<UserUpdateDto, UserUpdateModel>();

        CreateMap<UserModel, UserReadDto>()
            .AfterMap((_, dto) =>
            {
                foreach (var relationDto in dto.TopicStatuses)
                {
                    relationDto.User = dto;
                }
            });
    }
}