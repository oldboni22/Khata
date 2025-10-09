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
    public async Task<UserReadDto> CreateUserAsync(
        [FromBody] UserCreateDto userCreateDto, CancellationToken cancellationToken)
    {
        await createDtoValidator.ValidateAndThrowAsync(userCreateDto,cancellationToken);
        
        var model = mapper.Map<UserCreateModel>(userCreateDto);

        var createdUser = await userService.CreateAsync(model, cancellationToken);
        
        return mapper.Map<UserReadDto>(createdUser);
    }
    
    [HttpPost($"{UserIdRoute}/media")]
    public async Task UploadPictureAsync(
        IFormFile file, Guid userId, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();
        
        await userService.UpdatePictureAsync(senderId!, userId, file, cancellationToken);
    }
    
    [HttpGet("topics/{topicId}")]
    public async Task<PagedList<UserReadDto>> FindUsersAsync(
        [FromBody] UserTopicRelationStatus status,
        [FromQuery] PaginationParameters pagedParameters, 
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

    [HttpGet($"{UserIdRoute}/media")]
    public async Task<FileResult> FindPictureAsync(Guid userId, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();

        var (stream, stats) = await userService.FindUserPictureAsync(senderId!, userId, cancellationToken);

        return File(stream, stats.ContentType, stats.ObjectName);
    }
    
    [HttpGet($"{UserIdRoute}/relations")]
    public async Task<PagedList<UserTopicRelationReadDto>> FindUserRelationsAsync(
        [FromQuery] PaginationParameters paginationParameters , Guid userId, CancellationToken cancellationToken)
    {
        var relations = 
            await userService.FindUserRelationsAsync(userId, paginationParameters, cancellationToken);
        
        return mapper.Map<PagedList<UserTopicRelationReadDto>>(relations);
    }

    [Authorize]
    [HttpPut(UserIdRoute)]
    public async Task<UserReadDto> UpdateUserAsync(
        [FromBody] UserUpdateDto userUpdateDto, Guid userId, CancellationToken cancellationToken)
    {
        await updateDtoValidator.ValidateAndThrowAsync(userUpdateDto, cancellationToken);
        
        var model = mapper.Map<UserUpdateModel>(userUpdateDto);

        var senderId = User.GetAuth0Id();
        
        var updatedUser = await userService.UpdateAsync(senderId!, userId, model, cancellationToken);
        
        return mapper.Map<UserReadDto>(updatedUser);
    }

    [Authorize]
    [HttpDelete(UserIdRoute)]
    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();
        
        await userService.DeleteAsync(senderId! ,userId, cancellationToken);
    }
    
    #region Relations
    
    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/subscribe")]
    public async Task AddSubscriptionAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();
        
        await userService.AddSubscriptionAsync(senderId!, userId, topicId, cancellationToken);
    }
    
    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/unsubscribe")]
    public async Task RemoveSubscriptionAsync(
         Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();
        
        await userService.RemoveSubscriptionAsync(senderId!, userId, topicId, cancellationToken);
    }
    
    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/ban")]
    public async Task AddBanAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();
        
        await userService.AddBanAsync(senderId!, userId, topicId, cancellationToken);
    }

    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/unban")]
    public async Task RemoveBanAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();
        
        await userService.RemoveBanAsync(senderId!, userId, topicId, cancellationToken);
    }
    
    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/mod")]
    public async Task AddModerationStatusAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();
        
        await userService.AddModerationStatusAsync(senderId!, userId, topicId, cancellationToken);
    }
    
    [Authorize]
    [HttpPost($"{UserTopicRelationControlRoute}/unmod")]
    public async Task RemoveModerationStatusAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        var senderId = User.GetAuth0Id();
        
        await userService.RemoveModerationStatusAsync(senderId!, userId, topicId, cancellationToken);
    }
    
    #endregion
}
