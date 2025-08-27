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
    private const string UserTopicRelationControlRoute = "{userId}/topics/{topicId}";
    
    private const string UserIdRoute = "{userId}";
    
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
        [FromQuery] string status,
        Guid topicId,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<UserTopicRelationStatus>(status, out var relationStatus))
        {
            throw new BadHttpRequestException(InvalidStringLiteralExceptionMessageGenerator.GenerateMessage(status));
        }

        var models = await userService.FindUsersByTopicIdAsync(topicId, relationStatus, pagedParameters, cancellationToken);
        
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

    [HttpPut(UserIdRoute)]
    public async Task<UserReadDto> UpdateUserAsync([FromBody] UserUpdateDto userUpdateDto,  Guid userId, CancellationToken cancellationToken)
    {
        await updateDtoValidator.ValidateAndThrowAsync(userUpdateDto, cancellationToken);
        
        var model = mapper.Map<UserUpdateModel>(userUpdateDto);
        
        var updatedUser = await userService.UpdateAsync(userId, model, cancellationToken);
        
        return mapper.Map<UserReadDto>(updatedUser);
    }

    [HttpDelete(UserIdRoute)]
    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(userId, cancellationToken);
    }
    
    #region Relations
    
    [HttpPost($"{UserTopicRelationControlRoute}/subscribe")]
    public async Task AddSubscriptionAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        await userService.AddSubscriptionAsync(userId, topicId, cancellationToken);
    }
    
    [HttpPost($"{UserTopicRelationControlRoute}/unsubscribe")]
    public async Task RemoveSubscriptionAsync(
         Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        await userService.RemoveSubscriptionAsync(userId, topicId, cancellationToken);
    }
    
    [HttpPost($"{UserTopicRelationControlRoute}/ban")]
    public async Task AddBanAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        var moderId = User.GetSenderUserId()!.Value;
        
        await userService.AddBanAsync(moderId ,userId, topicId, cancellationToken);
    }

    [HttpPost($"{UserTopicRelationControlRoute}/unban")]
    public async Task RemoveBanAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        var moderId = User.GetSenderUserId()!.Value;
        
        await userService.RemoveBanAsync(moderId , userId, topicId, cancellationToken);
    }
    
    [HttpPost($"{UserTopicRelationControlRoute}/mod")]
    public async Task AddModerationStatusAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        await userService.AddModerationStatusAsync(userId, topicId, cancellationToken);
    }
    
    [HttpPost($"{UserTopicRelationControlRoute}/unmod")]
    public async Task RemoveModerationStatusAsync(Guid userId,  Guid topicId, CancellationToken cancellationToken)
    {
        await userService.RemoveModerationStatusAsync(userId, topicId, cancellationToken);
    }
    
    #endregion
}
