using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Extensions;
using Shared.PagedList;
using UserService.API.DTO;
using UserService.API.Exceptions;
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
    public async Task<UserReadDto> CreateUserAsync([FromBody] UserCreateDto userCreateDto, CancellationToken cancellationToken)
    {
        await createDtoValidator.ValidateAndThrowAsync(userCreateDto,cancellationToken);
        
        var model = mapper.Map<UserCreateModel>(userCreateDto);

        var createdUser = await userService.CreateAsync(model, cancellationToken);
        
        return mapper.Map<UserReadDto>(createdUser);
    }
    
    [HttpGet("topics/{topicId}")]
    public async Task<PagedList<UserReadDto>> FindUsersAsync(
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
        
        return mapper.Map<PagedList<UserReadDto>>(models);
    }

    [HttpGet("{id}")]
    public async Task<UserReadDto> FindUserAsync([FromQuery] Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(id, cancellationToken);
        
        return mapper.Map<UserReadDto>(user);
    }

    [HttpGet("{id}/relations")]
    public async Task<PagedList<UserTopicRelationDto>> FindUserRelationsAsync(
        [FromBody] PaginationParameters paginationParameters ,[FromQuery] Guid id, CancellationToken cancellationToken)
    {
        var relations = await userService.FindUserRelationsAsync(id, paginationParameters, cancellationToken);
        
        return mapper.Map<PagedList<UserTopicRelationDto>>(relations);
    }

    [HttpPut]
    public async Task<UserReadDto> UpdateUserAsync(
        [FromBody] UserUpdateDto userUpdateDto, [FromQuery] Guid id, CancellationToken cancellationToken)
    {
        await updateDtoValidator.ValidateAndThrowAsync(userUpdateDto, cancellationToken);
        
        var model = mapper.Map<UserUpdateModel>(userUpdateDto);
        
        var updatedUser = await userService.UpdateAsync(id, model, cancellationToken);
        
        return mapper.Map<UserReadDto>(updatedUser);
    }

    [HttpDelete]
    public async Task DeleteUserAsync([FromQuery] Guid id, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(id, cancellationToken);
    }
    
    #region Relations
    
    [HttpPost("{userId}/topics/{topicId}/subscribe")]
    public async Task AddSubscriptionAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.AddSubscriptionAsync(userId, topicId, cancellationToken);
    }
    
    [HttpPost("{userId}/topics/{topicId}/unsubscribe")]
    public async Task RemoveSubscriptionAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.RemoveSubscriptionAsync(userId, topicId, cancellationToken);
    }
    
    [HttpPost("{userId}/topics/{topicId}/ban")]
    public async Task AddBanAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        if(!User.TryGetSenderUserId(out var moderId))
        {
            throw new UnauthorizedException();
        }
        
        await userService.AddBanAsync(moderId ,userId, topicId, cancellationToken);
    }

    [HttpPost("{userId}/topics/{topicId}/unban")]
    public async Task RemoveBanAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        if (!User.TryGetSenderUserId(out var moderId))
        {
            throw new UnauthorizedException();
        }
        
        await userService.RemoveBanAsync(moderId , userId, topicId, cancellationToken);
    }
    
    [HttpPost("{userId}/topics/{topicId}/mod")]
    public async Task AddModerationStatusAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.AddModerationStatusAsync(userId, topicId, cancellationToken);
    }
    
    [HttpPost("{userId}/topics/{topicId}/unmod")]
    public async Task RemoveModerationStatusAsync(
        [FromQuery] Guid userId, [FromQuery] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.RemoveModerationStatusAsync(userId, topicId, cancellationToken);
    }
    
    #endregion
}
