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
public class UserController(
    IUserService userService,
    IMapper mapper, 
    IValidator<UserCreateDto> createDtoValidator,
    IValidator<UserUpdateDto> updateDtoValidator) : ControllerBase
{
    [HttpPost]
    public async Task<UserReadDto> CreateUserAsync(
        [FromBody] UserCreateDto userCreateDto, CancellationToken cancellationToken)
    {
        await createDtoValidator.ValidateAndThrowAsync(userCreateDto,cancellationToken);
        
        var model = mapper.Map<UserCreateModel>(userCreateDto);

        var createdUser = await userService.CreateAsync(model, cancellationToken);
        
        return mapper.Map<UserReadDto>(createdUser);
    }
    
    [HttpGet("topics/{topicId}")]
    public async Task<PagedList<UserReadDto>> FindUsersAsync(
        [FromBody] PaginationParameters pagedParameters, 
        [FromRoute] Guid topicId, 
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

    [HttpGet("{userId}")]
    public async Task<UserReadDto> FindUserAsync([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(userId, cancellationToken);
        
        return mapper.Map<UserReadDto>(user);
    }

    [HttpGet("{userId}/relations")]
    public async Task<PagedList<UserTopicRelationDto>> FindUserRelationsAsync(
        [FromBody] PaginationParameters paginationParameters ,[FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var relations = 
            await userService.FindUserRelationsAsync(userId, paginationParameters, cancellationToken);
        
        return mapper.Map<PagedList<UserTopicRelationDto>>(relations);
    }

    [HttpPut("{userId}")]
    public async Task<UserReadDto> UpdateUserAsync(
        [FromBody] UserUpdateDto userUpdateDto, [FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        await updateDtoValidator.ValidateAndThrowAsync(userUpdateDto, cancellationToken);
        
        var model = mapper.Map<UserUpdateModel>(userUpdateDto);
        
        var updatedUser = await userService.UpdateAsync(userId, model, cancellationToken);
        
        return mapper.Map<UserReadDto>(updatedUser);
    }

    [HttpDelete]
    public async Task DeleteUserAsync([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(userId, cancellationToken);
    }
    
    #region Relations
    
    [HttpPost("{userId}/topics/{topicId}/subscribe")]
    public async Task AddSubscriptionAsync(
        [FromRoute] Guid userId, [FromRoute] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.AddSubscriptionAsync(userId, topicId, cancellationToken);
    }
    
    [HttpPost("{userId}/topics/{topicId}/unsubscribe")]
    public async Task RemoveSubscriptionAsync(
        [FromRoute] Guid userId, [FromRoute] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.RemoveSubscriptionAsync(userId, topicId, cancellationToken);
    }
    
    [HttpPost("{userId}/topics/{topicId}/ban")]
    public async Task AddBanAsync(
        [FromRoute] Guid userId, [FromRoute] Guid topicId, CancellationToken cancellationToken)
    {
        if(!User.TryGetSenderUserId(out var moderId))
        {
            throw new UnauthorizedException();
        }
        
        await userService.AddBanAsync(moderId ,userId, topicId, cancellationToken);
    }

    [HttpPost("{userId}/topics/{topicId}/unban")]
    public async Task RemoveBanAsync(
        [FromRoute] Guid userId, [FromRoute] Guid topicId, CancellationToken cancellationToken)
    {
        if (!User.TryGetSenderUserId(out var moderId))
        {
            throw new UnauthorizedException();
        }
        
        await userService.RemoveBanAsync(moderId , userId, topicId, cancellationToken);
    }
    
    [HttpPost("{userId}/topics/{topicId}/mod")]
    public async Task AddModerationStatusAsync(
        [FromRoute] Guid userId, [FromRoute] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.AddModerationStatusAsync(userId, topicId, cancellationToken);
    }
    
    [HttpPost("{userId}/topics/{topicId}/unmod")]
    public async Task RemoveModerationStatusAsync(
        [FromRoute] Guid userId, [FromRoute] Guid topicId, CancellationToken cancellationToken)
    {
        await userService.RemoveModerationStatusAsync(userId, topicId, cancellationToken);
    }
    
    #endregion
}
