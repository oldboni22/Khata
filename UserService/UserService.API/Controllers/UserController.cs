using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userCreateDto, CancellationToken cancellationToken)
    {
        await createDtoValidator.ValidateAndThrowAsync(userCreateDto,cancellationToken);
        
        var model = mapper.Map<UserCreateModel>(userCreateDto);

        await userService.CreateAsync(model, cancellationToken);
        
        return Ok();
    }
}
