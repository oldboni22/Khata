using Messages.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Moq;
using NotificationService.API.Services;
using NotificationService.Domain.Contracts;
using NotificationService.Domain.Contracts.Repos;
using NotificationService.Infrastructure.GRpc;
using NotificationService.Infrastructure.MemoryCache;
using NotificationService.Infrastructure.Socket;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.PagedList;
using Shouldly;

namespace NotificationService.Tests;

public class NotificationServiceTests
{
    private readonly INotificationService _notificationService;

    private readonly Mock<INotificationRepository> _repositoryMock;
    
    private readonly Mock<TimeProvider> _timeProviderMock;
    
    private readonly Mock<IUserGrpcService> _userGrpcServiceMock;

    private readonly Mock<IHubContext<NotificationHub>> _hubContext;

    private readonly Mock<IMemoryCacheService<Guid, string>> _userIdMemoryCacheMock;

    private readonly Mock<IOptions<NotificationServiceSocketOptions>> _socketOptionsMock;
    
    public NotificationServiceTests()
    {
        _repositoryMock = new();
        _timeProviderMock = new();
        _userGrpcServiceMock = new();
        _hubContext = new();
        _userIdMemoryCacheMock = new();
        _socketOptionsMock = new();
        
        _notificationService = new API.Services.NotificationService(
            _repositoryMock.Object,
            _timeProviderMock.Object,
            _userGrpcServiceMock.Object,
            _hubContext.Object,
            _userIdMemoryCacheMock.Object,
            _socketOptionsMock.Object);
    }

    [Fact]
    public async Task CreateNotificationsAsync_EmptyList()
    {
        var notifications = new List<Notification> { };
        
        await _notificationService.CreateNotificationsAsync(notifications);
        
        _repositoryMock.Verify(repo => repo.CreateManyAsync(notifications), Times.Once);
    }
    
    [Fact]
    public async Task CreateNotificationsAsync_NotEmptyList()
    {
        var notifications = new List<Notification> { new Notification() };
        
        await _notificationService.CreateNotificationsAsync(notifications);
        
        _repositoryMock.Verify(repo => repo.CreateManyAsync(notifications), Times.Once);
    }
    
    [Fact]
    public async Task FindAllNotificationsAsync_NotificationsExist_ReturnsNotifications()
    {
        var userId = Guid.NewGuid();
        
        _userGrpcServiceMock
            .Setup(x => x.GetUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(userId);

        var paginationParameters = new PaginationParameters(1,10);
        
        var pagedNotifications = new List<Notification>
        {
            new Notification
            {
                UserId = userId,
            }
        }.ToPagedList(1,10,1,1);
        
        _repositoryMock
            .Setup(x =>
                x.FindAllNotificationsAsync(userId, paginationParameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedNotifications);
        
        //Act
        var result = await _notificationService.FindAllNotificationsAsync("", paginationParameters);
        
        //Assert
        result.ShouldBe(pagedNotifications);
    }
    
    [Fact]
    public async Task FindAllNotificationsAsync_NotificationsDontExist_ReturnsNotifications()
    {
        var userId = Guid.NewGuid();
        
        _userGrpcServiceMock
            .Setup(x => x.GetUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(userId);

        var paginationParameters = new PaginationParameters(1,10);
        
        var pagedNotifications = new List<Notification>{}
            .ToPagedList(1,10,1,1);
        
        _repositoryMock
            .Setup(x =>
                x.FindAllNotificationsAsync(userId, paginationParameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedNotifications);
        
        //Act
        var result = await _notificationService.FindAllNotificationsAsync("", paginationParameters);
        
        //Assert
        result.ShouldBe(pagedNotifications);
    }
    
    [Fact]
    public async Task FindUnreadNotificationsAsync_NotificationsExist_ReturnsNotifications()
    {
        var userId = Guid.NewGuid();
        
        _userGrpcServiceMock
            .Setup(x => x.GetUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(userId);

        var paginationParameters = new PaginationParameters(1,10);
        
        var pagedNotifications = new List<Notification>
        {
            new Notification
            {
                UserId = userId,
                ReadAt = null
            }
        }.ToPagedList(1,10,1,1);
        
        _repositoryMock
            .Setup(x =>
                x.FindUnreadNotificationsAsync(userId, paginationParameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedNotifications);
        
        //Act
        var result = await _notificationService.FindUnreadNotificationsAsync("", paginationParameters);
        
        //Assert
        result.ShouldBe(pagedNotifications);
    }
    
    [Fact]
    public async Task FindUnreadNotificationsAsync_NotificationsDontExist_ReturnsNotifications()
    {
        var userId = Guid.NewGuid();
        
        _userGrpcServiceMock
            .Setup(x => x.GetUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(userId);

        var paginationParameters = new PaginationParameters(1,10);
        
        var pagedNotifications = new List<Notification>{}
            .ToPagedList(1,10,1,1);
        
        _repositoryMock
            .Setup(x =>
                x.FindUnreadNotificationsAsync(userId, paginationParameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedNotifications);
        
        //Act
        var result = await _notificationService.FindUnreadNotificationsAsync(
            "", paginationParameters);
        
        //Assert
        result.ShouldBe(pagedNotifications);
    }

    [Fact]
    public async Task MarkNotificationAsReadAsync_NotificationDoesNotExistsAndUnread_ThrowsNotFound()
    {
        var notificationId = Guid.NewGuid();

        var result = _notificationService.MarkNotificationAsReadAsync(notificationId);
        
        await Should.ThrowAsync<NotFoundException>(result);
    }

    [Fact]
    public async Task MarkNotificationAsReadAsync_NotificationExistsAndRead_ThrowsBadRequest()
    {
        var notificationId = Guid.NewGuid();
        
        var notification = new Notification
        {
            Id = notificationId,
            ReadAt = DateTime.UtcNow
        };
        
        _repositoryMock
            .Setup(x => x.FindById(notificationId))
            .ReturnsAsync(notification);

        var result = _notificationService.MarkNotificationAsReadAsync(notificationId);
        
        await Should.ThrowAsync<BadRequestException>(result);
    }

    [Fact]
    public async Task MarkNotificationAsReadAsync_NotificationExistsAndUnread_MarksAsRead()
    {
        var notificationId = Guid.NewGuid();
        
        var notification = new Notification
        {
            Id = notificationId,
            ReadAt = null
        };
        
        _repositoryMock
            .Setup(x => x.FindById(notificationId))
            .ReturnsAsync(notification);

        await _notificationService.MarkNotificationAsReadAsync(notificationId);
        
        _repositoryMock.Verify(repo => repo.UpdateAsync(notification), Times.Once);
    }

    [Fact]
    public async Task MarkUnreadNotificationsAsReadAsync_NotificationExistsAndUnread_MarksAsRead()
    {
        var userId = Guid.NewGuid();
        
        var notifications = new List<Notification>();
        
        _userGrpcServiceMock
            .Setup(x => x.GetUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(userId);

        _repositoryMock
            .Setup(x => x.FindUnreadNotificationsAsync(userId,It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);
        
        await _notificationService.MarkUnreadNotificationsAsReadAsync("");
        
        _repositoryMock.Verify(repo => repo.UpdateManyAsync(notifications), Times.Once);
        _timeProviderMock.Verify(timeProvider => timeProvider.GetUtcNow(), Times.Once);
    }
    
}