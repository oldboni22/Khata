using System.Text.Json.Serialization;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Extensions;
using Shared.PagedList;
using UserService.API.DTO;
using UserService.BLL.Models.User;
using UserService.BLL.Services;
using UserService.API.ActionFilters;
using UserService.API.Utilities.ApiKeys;

namespace UserService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(
    IUserService userService,
    IMapper mapper, 
    IValidator<UserCreateDto> createDtoValidator,
    IValidator<UserUpdateDto> updateDtoValidator) : ControllerBase
{
    private const string UserTopicRelationControlRoute = "{userId}/topics/{topicId}";
    
    private const string UserIdRoute = "{userId}";
    
    [HttpPost]
    [ApiKeyFilter(ApiType.Auth0)]
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
        [FromQuery] UserTopicRelationStatus status,
        Guid topicId,
        CancellationToken cancellationToken)
    {
        var models = 
            await userService.FindUsersByTopicIdAsync(topicId, status, pagedParameters, cancellationToken);
        
        return mapper.Map<PagedList<UserReadDto>>(models);
    }

    [HttpGet(UserIdRoute)]
    public async Task<UserReadDto> FindUserAsync( Guid userId, CancellationToken cancellationToken)
    {
        var user = await userService.FindByIdAsync(userId, cancellationToken);
        
        return mapper.Map<UserReadDto>(user);
    }

    [HttpGet($"{UserIdRoute}/relations")]
    public async Task<PagedList<UserTopicRelationDto>> FindUserRelationsAsync(
        [FromBody] PaginationParameters paginationParameters , Guid userId, CancellationToken cancellationToken)
    {
        var relations = 
            await userService.FindUserRelationsAsync(userId, paginationParameters, cancellationToken);
        
        return mapper.Map<PagedList<UserTopicRelationDto>>(relations);
    }

    [Authorize]
    [HttpPut(UserIdRoute)]
    public async Task<UserReadDto> UpdateUserAsync(
        [FromBody] UserUpdateDto userUpdateDto, Guid userId, CancellationToken cancellationToken)
    {
        await updateDtoValidator.ValidateAndThrowAsync(userUpdateDto, cancellationToken);
        
        var model = mapper.Map<UserUpdateModel>(userUpdateDto);
        
        var senderId = User.GetId()!.Value;
        
        var updatedUser = await userService.UpdateAsync(senderId, userId, model, cancellationToken);
        
        return mapper.Map<UserReadDto>(updatedUser);
    }

    [Authorize]
    [HttpDelete(UserIdRoute)]
    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var senderId = User.GetId()!.Value;
        
        await userService.DeleteAsync(senderId ,userId, cancellationToken);
    }
    
    #region Relations
    
    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/subscribe")]
    public async Task AddSubscriptionAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        await userService.AddSubscriptionAsync(userId, topicId, cancellationToken);
    }
    
    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/unsubscribe")]
    public async Task RemoveSubscriptionAsync(
         Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        await userService.RemoveSubscriptionAsync(userId, topicId, cancellationToken);
    }
    
    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/ban")]
    public async Task AddBanAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        var moderId = User.GetId()!.Value;
        
        await userService.AddBanAsync(moderId ,userId, topicId, cancellationToken);
    }

    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/unban")]
    public async Task RemoveBanAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        var moderId = User.GetId()!.Value;
        
        await userService.RemoveBanAsync(moderId , userId, topicId, cancellationToken);
    }
    
    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/mod")]
    public async Task AddModerationStatusAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        await userService.AddModerationStatusAsync(userId, topicId, cancellationToken);
    }
    
    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/unmod")]
    public async Task RemoveModerationStatusAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        await userService.RemoveModerationStatusAsync(userId, topicId, cancellationToken);
    }
    
    #endregion
}
