using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.PagedList;
using UserService.API.DTO;
using UserService.BLL.Models.User;
using UserService.BLL.Services;
using UserService.DAL.Models.Entities;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/user")]
public class UserController(IUserService userService, IMapper mapper, 
    IValidator<UserCreateDto> createDtoValidator, IValidator<UserUpdateDto> updateDtoValidator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserCreateDto userCreateDto, 
        CancellationToken cancellationToken)
    {
        await createDtoValidator.ValidateAndThrowAsync(userCreateDto,cancellationToken);
        
        var model = mapper.Map<UserCreateModel>(userCreateDto);

        var created = await userService.CreateAsync(model, cancellationToken);
        
        return Ok(mapper.Map<UserReadDto>(created));
    }
    
    [HttpGet]
    public async Task<IActionResult> FindUsersByTopicIdAsync(
        [FromQuery] Guid topicId, 
        [FromQuery] string statusString,
        [FromBody] PagedListQueryParameters pagedParameters, 
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserTopicRelationStatus>(statusString, out var status))
        {
            throw new ArgumentException();
        }

        var models = await userService.FindUsersByTopicIdAsync(topicId, status, pagedParameters, cancellationToken);
        
        return Ok(mapper.Map<PagedList<UserReadDto>>(models));
    }
    
}
