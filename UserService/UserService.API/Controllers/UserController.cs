using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Extensions;
using Shared.PagedList;
using UserService.API.DTO;
using UserService.API.Utilities.MessageGenerators.Exceptions;
using UserService.BLL.Models.User;
using UserService.BLL.Services;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService, IMapper mapper, 
    IValidator<UserCreateDto> createDtoValidator, IValidator<UserUpdateDto> updateDtoValidator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserCreateDto userCreateDto, CancellationToken cancellationToken)
    {
        await createDtoValidator.ValidateAndThrowAsync(userCreateDto,cancellationToken);
        
        var model = mapper.Map<UserCreateModel>(userCreateDto);

        var createdUser = await userService.CreateAsync(model, cancellationToken);
        
        return Ok(mapper.Map<UserReadDto>(createdUser));
    }
    
    [HttpGet("topics/{topicId}")]
    public async Task<IActionResult> FindUsersAsync(
        [FromBody] PaginationParameters pagedParameters, 
        [FromQuery] Guid topicId, 
        [FromQuery] string status,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserTopicRelationStatus>(status, out var relationStatus))
        {
            throw new BadHttpRequestException(InvalidStringLiteralExceptionMessageGenerator.GenerateMessage(status));
        }

        var models = await userService.FindUsersByTopicIdAsync(topicId, relationStatus, pagedParameters, cancellationToken);
        
        return Ok(mapper.Map<PagedList<UserReadDto>>(models));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> FindUserAsync([FromQuery] Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(id, cancellationToken);
        
        return Ok(mapper.Map<UserReadDto>(user));
    }

    [HttpGet("{id}/relations")]
    public async Task<IActionResult> FindUserRelationsAsync(
        [FromBody] PaginationParameters paginationParameters ,[FromQuery] Guid id, CancellationToken cancellationToken)
    {
        var relations = await userService.FindUserRelationsAsync(id, paginationParameters, cancellationToken);
        
        return Ok(relations);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserAsync(
        [FromBody] UserUpdateDto userUpdateDto, [FromQuery] Guid id, CancellationToken cancellationToken)
    {
        await updateDtoValidator.ValidateAndThrowAsync(userUpdateDto, cancellationToken);
        
        var model = mapper.Map<UserUpdateModel>(userUpdateDto);
        
        var updatedUser = await userService.UpdateAsync(id, model, cancellationToken);
        
        return Ok(mapper.Map<UserReadDto>(updatedUser));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUserAsync([FromQuery] Guid id, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(id, cancellationToken);
        
        return NoContent();
    }
    
    #region Relations
    
    [HttpPost("{userId}/topics/subscribe")]
    public async Task<IActionResult> AddSubscriptionAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.AddSubscriptionAsync(userId, topicId, cancellationToken);
        
        return Ok();
    }
    
    [HttpPost("{userId}/topics/unsubscribe")]
    public async Task<IActionResult> RemoveSubscriptionAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.RemoveSubscriptionAsync(userId, topicId, cancellationToken);
        
        return Ok();
    }
    
    [HttpPost("{userId}/topics/ban")]
    public async Task<IActionResult> AddBanAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        if(!User.TryGetSenderUserId(out var moderId))
        {
            return Unauthorized();
        }
        
        await userService.AddBanAsync(moderId ,userId, topicId, cancellationToken);
        
        return Ok();
    }

    [HttpPost("{userId}/topics/unban")]
    public async Task<IActionResult> RemoveBanAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        if (!User.TryGetSenderUserId(out var moderId))
        {
            return Unauthorized();
        }
        
        await userService.RemoveBanAsync(moderId , userId, topicId, cancellationToken);

        return Ok();
    }
    
    [HttpPost("{userId}/topics/mod")]
    public async Task<IActionResult> AddModerationStatusAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.AddModerationStatusAsync(userId, topicId, cancellationToken);

        return Ok();
    }
    
    [HttpPost("{userId}/topics/unmod")]
    public async Task<IActionResult> RemoveModerationStatusAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.RemoveModerationStatusAsync(userId, topicId, cancellationToken);

        return Ok();
    }
    
    #endregion

    private bool TryGetSenderUserId(out Guid senderId)
    {
        var moderIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        senderId = Guid.Empty;

        return string.IsNullOrEmpty(moderIdString) && !Guid.TryParse(moderIdString, out senderId);
    }
}
