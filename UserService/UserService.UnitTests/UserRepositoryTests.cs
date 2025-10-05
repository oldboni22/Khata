using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shouldly;
using UserService.DAL;
using UserService.DAL.CacheService;
using UserService.DAL.Models.Entities;
using UserService.DAL.Repositories;
using UserService.IntegrationTests.Extensions;
using UserRepository = UserService.DAL.Repositories.UserRepository;

namespace UserService.UnitTests;

public class UserRepositoryTests
{
    private readonly IFixture _fixture;
    
    private readonly UserServiceContext _userServiceContextMock;
    
    private readonly IUserAuth0IdCacheService _userAuth0IdCacheServiceMock = Substitute.For<IUserAuth0IdCacheService>();

    private readonly IUserRepository _userRepository;
    
    public UserRepositoryTests()
    {
        var contextOptions = new DbContextOptionsBuilder<UserServiceContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _userServiceContextMock = new UserServiceContext(contextOptions);
        
        _fixture = new Fixture();
        
        _userRepository = new UserRepository(_userServiceContextMock, _userAuth0IdCacheServiceMock);
    }

    [Fact]
    public async Task FindUserByAuth0IdAsync_UserExistsInDbButDoesNotExistInCache_CallsSetValueFromCacheService()
    {
        //Assert
        var user = _fixture.CreateUser();
        var auth0Id = user.Auth0Id;
     
        await SeedDatabase(new List<User>() { user });
        
        _userAuth0IdCacheServiceMock.TryGetValueAsync(auth0Id).Returns(null, user);
        
        //Act
        var result1 = await _userRepository.FindUserByAuth0IdAsync(auth0Id);
        var result2 = await _userRepository.FindUserByAuth0IdAsync(auth0Id);
        
        //Assert
        result1.ShouldNotBeNull();
        result2.ShouldNotBeNull();
        result1.ShouldBeEquivalentTo(result2);
        
        await _userAuth0IdCacheServiceMock.Received(2).TryGetValueAsync(auth0Id);
        await _userAuth0IdCacheServiceMock.Received(1).SetValueAsync(Arg.Is<User>(u => u.Id == user.Id));
    }

    [Fact]
    public async Task FindUserByAuth0IdAsync_UserDoesNotExistInDb_ReturnsNull()
    {
        //Assert
        var auth0Id = "fake_id";
        
        //Act 
        var result1 = await _userRepository.FindUserByAuth0IdAsync(auth0Id);
        var result2 = await _userRepository.FindUserByAuth0IdAsync(auth0Id);
        
        //Assert
        result1.ShouldBeNull();
        result2.ShouldBeNull();
        
        await _userAuth0IdCacheServiceMock.Received(2).TryGetValueAsync(auth0Id);
        await _userAuth0IdCacheServiceMock.Received(0).SetValueAsync(Arg.Any<User>());
    }

    private async Task SeedDatabase(IEnumerable<User> users)
    {
        _userServiceContextMock.Users.AddRange(users);
        await _userServiceContextMock.SaveChangesAsync();
    }
}
