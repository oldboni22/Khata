using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using MinIoService;
using Shared;
using UserService.API.DTO;
using UserService.BLL.gRpc;
using Shouldly;
using UserService.DAL;
using UserService.DAL.Models.Entities;
using UserService.IntegrationTests.Extensions;
using UserService.IntegrationTests.Utils;
using Xunit.Abstractions;

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
        
        var scope = factory.Services.CreateScope();
        
        var context = scope.ServiceProvider.GetRequiredService<UserServiceContext>();
        
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        _seededUser = _fixture.CreateUser();
        context.Set<User>().Add(_seededUser);
        
        context.SaveChanges();
        
        scope.Dispose();
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
    }
    
    [Fact]
    public async Task UploadPictureAsync_NotMatchingAuth0Id_Forbidden()
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
    public async Task UploadPictureAsync_UserDoesNotExist_NotFound()
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

    private void SetConfiguration()
    {
        Dictionary<string,string> envVariables = new()
        {
            {ConfigurationKeys.MinioAccessKey, "Fake_Key"},
            {ConfigurationKeys.MinioSecretKey, "Fake_Key"},
            {ConfigurationKeys.MinioEndpoint, "fakeHost:1111"},
            
            {ConfigurationKeys.TopicGRpcPort, "1234"},
            {ConfigurationKeys.UserGRpcPort, "1234"},
            {ConfigurationKeys.TopicGRpcAddress, "https://fakehost:1234"},
            {ConfigurationKeys.UserGRpcAddress, "https://fakehost:1234"},
            
            {ConfigurationKeys.ApplicationPort, "1234"},
        };

        foreach (var (key, value) in envVariables)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
