using System.Text.Json;
using AutoFixture;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using UserService.DAL.CacheService;
using UserService.DAL.Models.Entities;
using UserService.IntegrationTests.Extensions;

namespace UserService.UnitTests;

public class UserAuth0IdCacheServiceTests
{
    private static string CreateKey(User user) => $"U_auth0_{user.Auth0Id}";
    
    private readonly IUserAuth0IdCacheService _userAuth0IdCacheService;
    
    private readonly IDistributedCache _distributedCacheMock = Substitute.For<IDistributedCache>();
    
    private readonly IOptions<CacheServiceOptions> _optionsMock = Substitute.For<IOptions<CacheServiceOptions>>();

    private readonly IFixture _fixture = new Fixture();
    
    public UserAuth0IdCacheServiceTests()
    {
        _optionsMock.Value.Returns(new CacheServiceOptions { Lifetime = "1"} );

        _userAuth0IdCacheService = new UserAuth0IdCacheService(_optionsMock, _distributedCacheMock);
    }

    [Fact]
    public async Task TryGetValueAsync_ValueExists_ReturnsValue()
    {
        //Assert
        var user = _fixture.CreateUser();
        var userJson = JsonSerializer.Serialize(user);
        var userBytes = System.Text.Encoding.UTF8.GetBytes(userJson);
        
        _distributedCacheMock.GetAsync(CreateKey(user)).Returns(userBytes);
        
        //Act
        var result = await _userAuth0IdCacheService.TryGetValueAsync(user.Auth0Id);
        
        //Assert
        result.ShouldNotBeNull();
        result.ShouldBeEquivalentTo(user);
    }
    
    [Fact]
    public async Task TryGetValueAsync_ValueDoesNotExist_ReturnsNull()
    {
        //Act
        var result = await _userAuth0IdCacheService.TryGetValueAsync("Some_not_existing_auth0id");
        
        //Assert
        result.ShouldBeNull();
    }
}
