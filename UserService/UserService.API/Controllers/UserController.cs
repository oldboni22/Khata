using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.PagedList;
using UserService.API.DTO;
using UserService.API.Exceptions;
using UserService.API.Utilities.MessageGenerators.Exceptions;
using UserService.BLL.Models.User;
using UserService.BLL.Services;
using UserService.DAL.Models.Entities;

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
    public async Task<IActionResult> SubscribeToTopicAsync
        ([FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.SubscribeUserAsync(userId, topicId, cancellationToken);
        
        return Ok();
    }
    
    [HttpPost("{userId}/topics/unsubscribe")]
    public async Task<IActionResult> UnsubscribeFromTopicAsync
        ([FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.UnsubscribeUserAsync(userId, topicId, cancellationToken);
        
        return Ok();
    }
    
    [HttpPost("{userId}/topics/ban")]
    public async Task<IActionResult> BanUserFromTopicAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.BanUserAsync(userId, topicId, cancellationToken);
        
        return Ok();
    }

    [HttpPost("{userId}/topics/unban")]
    public async Task<IActionResult> UnbanUserFromTopicAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.UnbanUserAsync(userId, topicId, cancellationToken);

        return Ok();
    }
    
    [HttpPost("{userId}/topics/mod")]
    public async Task<IActionResult> PromoteUserToModeratorAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.PromoteUserToModeratorAsync(userId, topicId, cancellationToken);

        return Ok();
    }
    
    [HttpPost("{userId}/topics/unmod")]
    public async Task<IActionResult> DemoteUserFromModeratorAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.DemoteUserFromModeratorAsync(userId, topicId, cancellationToken);

        return Ok();
    }
    
    #endregion
}
