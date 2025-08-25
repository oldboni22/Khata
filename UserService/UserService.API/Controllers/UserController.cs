using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.PagedList;
using UserService.API.DTO;
using UserService.API.Exceptions;
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
        [FromBody] PaginationParameters pagedParameters, 
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserTopicRelationStatus>(statusString, out var status))
        {
            throw new InvalidRelationStringLiteralException(statusString);
        }

        var models = await userService.FindUsersByTopicIdAsync(topicId, status, pagedParameters, cancellationToken);
        
        return Ok(mapper.Map<PagedList<UserReadDto>>(models));
    }

    [HttpGet]
    public async Task<IActionResult> FindUserByIdAsync([FromQuery] Guid id, CancellationToken cancellationToken = default)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        
        return Ok(mapper.Map<UserReadDto>(user));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserAsync([FromQuery] Guid id, [FromBody] UserUpdateDto userUpdateDto,
        CancellationToken cancellationToken)
    {
        await updateDtoValidator.ValidateAndThrowAsync(userUpdateDto, cancellationToken);
        
        var model = mapper.Map<UserUpdateModel>(userUpdateDto);
        
        var updated = await userService.UpdateAsync(id, model, cancellationToken);
        
        return Ok(mapper.Map<UserReadDto>(updated));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUserAsync([FromQuery] Guid id, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(id, cancellationToken);
        
        return NoContent();
    }
    
    #region Relations
    
    [HttpPost("/subscribe")]
    public async Task<IActionResult> SubscribeToTopicAsync([FromQuery] Guid userId, [FromQuery] Guid topicId, 
        CancellationToken cancellationToken)
    {
        await userService.SubscribeUserAsync(userId, topicId, cancellationToken);
        
        return Ok();
    }
    
    [HttpPost("/unsubscribe")]
    public async Task<IActionResult> UnsubscribeToTopicAsync([FromQuery] Guid userId, [FromQuery] Guid topicId, 
        CancellationToken cancellationToken)
    {
        await userService.UnsubscribeUserAsync(userId, topicId, cancellationToken);
        
        return Ok();
    }
    
    [HttpPost("/ban")]
    public async Task<IActionResult> BanUserFromTopicAsync([FromQuery] Guid userId, [FromQuery] Guid topicId, 
        CancellationToken cancellationToken)
    {
        await userService.BanUserAsync(userId, topicId, cancellationToken);
        
        return Ok();
    }

    [HttpPost("/unban")]
    public async Task<IActionResult> UnbanUserFromTopicAsync([FromQuery] Guid userId, [FromQuery] Guid topicId,
        CancellationToken cancellationToken)
    {
        await userService.UnbanUserAsync(userId, topicId, cancellationToken);

        return Ok();
    }
    
    [HttpPost("/mod")]
    public async Task<IActionResult> PromoteUserToModeratorAsync([FromQuery] Guid userId, [FromQuery] Guid topicId,
        CancellationToken cancellationToken)
    {
        await userService.PromoteUserToModeratorAsync(userId, topicId, cancellationToken);

        return Ok();
    }
    
    [HttpPost("/unmod")]
    public async Task<IActionResult> DemoteUserFromModeratorAsync([FromQuery] Guid userId, [FromQuery] Guid topicId,
        CancellationToken cancellationToken)
    {
        await userService.DemoteUserFromModeratorAsync(userId, topicId, cancellationToken);

        return Ok();
    }
    
    #endregion
}
