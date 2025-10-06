using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MinIoService;
using NSubstitute;
using Shared;
using UserService.API.DTO;
using UserService.BLL.gRpc;
using Shouldly;
using UserService.DAL;
using UserService.DAL.CacheService;
using UserService.DAL.Models.Entities;
using UserService.IntegrationTests.Extensions;
using UserService.IntegrationTests.Utils;
using Xunit.Abstractions;
using Program = UserService.API.Program;

namespace UserService.IntegrationTests;

public class UserControllerTests : IClassFixture<UserServiceTestFactory>, IClassFixture<AutoFixtureContainer>
{
    private readonly ITestOutputHelper _output;
    
    private readonly HttpClient _client;
    
    private readonly IMinioService _minioServiceMock;
    
    private readonly ITopicGRpcClient _topicGRpcClientMock;
    
    private readonly IFixture _fixture;

    private readonly User _seededUser;
    
    public UserControllerTests(UserServiceTestFactory factory, AutoFixtureContainer fixtureContainer, ITestOutputHelper output)
    {
        SetConfiguration();
        
        _output = output;
        
        _fixture = fixtureContainer.Fixture;
        
        _client = factory.CreateClient();
        
        factory.ClearMocks();

        _minioServiceMock = factory.MinioServiceMock;
        _topicGRpcClientMock = factory.TopicGRpcClientMock;
        
        _seededUser = _fixture.CreateUser();
        SeedDatabase(factory, _seededUser);
    }

    [Fact]
    public async Task CreateUserAsync_ValidApiKey_ReturnsCreatedUser()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-KEY", UserServiceTestFactory.Auth0ApiKey);
        
        var userCreateDto = _fixture.CreateUserCreateDto();
        
        // Act
        var response = await _client.PostAsJsonAsync("api/user", userCreateDto);
        
        var responseStr = await response.Content.ReadAsStringAsync();
        var createdUser = JsonSerializer.Deserialize<UserReadDto>(
            responseStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true});
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        createdUser.ShouldNotBeNull();
        createdUser.Name.ShouldBe(userCreateDto.Name);
    }
    
    [Fact]
    public async Task CreateUserAsync_InvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-KEY", "qwe");
        
        var userCreateDto = _fixture.CreateUserCreateDto();
        
        // Act
        var response = await _client.PostAsJsonAsync("api/user", userCreateDto);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task UploadPictureAsync_ValidRequest_UploadsPicture()
    {
        // Arrange
        var userId = _seededUser.Id;
        _client.DefaultRequestHeaders.Add(TestAuthenticationHandler.ReturnIdHeader, _seededUser.Auth0Id);
        
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "test.jpg";
        
        using var content = new MultipartFormDataContent();
        using var fileContentContent = new ByteArrayContent(fileContent);
        
        fileContentContent.Headers.ContentType = 
            new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        
        content.Add(fileContentContent, "file", fileName);
        
        // Act
        var response = await _client.PostAsync($"api/user/{userId}/media", content);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await _minioServiceMock.Received(1).UploadFileAsync(Arg.Any<IFormFile>(), userId.ToString());
    }
    
    [Fact]
    public async Task UploadPictureAsync_NotMatchingAuth0Id_ReturnsForbidden()
    {
        // Arrange
        var userId = _seededUser.Id;
        _client.DefaultRequestHeaders.Add(TestAuthenticationHandler.ReturnIdHeader, "some_other_id");
        
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "test.jpg";
        
        using var content = new MultipartFormDataContent();
        using var fileContentContent = new ByteArrayContent(fileContent);
        
        fileContentContent.Headers.ContentType = 
            new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        
        content.Add(fileContentContent, "file", fileName);
        
        // Act
        var response = await _client.PostAsync($"api/user/{userId}/media", content);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task UploadPictureAsync_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _client.DefaultRequestHeaders.Add(TestAuthenticationHandler.ReturnIdHeader, "some_other_id");
        
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "test.jpg";
        
        using var content = new MultipartFormDataContent();
        using var fileContentContent = new ByteArrayContent(fileContent);
        
        fileContentContent.Headers.ContentType = 
            new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        
        content.Add(fileContentContent, "file", fileName);
        
        // Act
        var response = await _client.PostAsync($"api/user/{userId}/media", content);
        
        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static void SeedDatabase(WebApplicationFactory<Program> factory, User user)
    {
        var scope = factory.Services.CreateScope();
        
        var context = scope.ServiceProvider.GetRequiredService<UserServiceContext>();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        context.Set<User>().Add(user);
        
        context.SaveChanges();
        
        scope.Dispose();
    }
    
    private static void SetConfiguration()
    {
        Dictionary<string,string> envVariables = new()
        {
            {ConfigurationKeys.MinioAccessKey, "Fake_Key"},
            {ConfigurationKeys.MinioSecretKey, "Fake_Key"},
            {ConfigurationKeys.MinioEndpoint, "fakeHost:1111"},
            
            {ConfigurationKeys.TopicGRpcPort, "1111"},
            {ConfigurationKeys.UserGRpcPort, "1234"},
            {ConfigurationKeys.TopicGRpcAddress, "https://fakehost:1111"},
            {ConfigurationKeys.UserGRpcAddress, "https://fakehost:1111"},
            
            {ConfigurationKeys.ApplicationPort, "1234"},
        };

        foreach (var (key, value) in envVariables)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
