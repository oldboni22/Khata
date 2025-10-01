using System.Text;
using AutoFixture;
using UserService.API.DTO;
using UserService.DAL.Models.Entities;
using UserService.IntegrationTests.Utils;

namespace UserService.IntegrationTests.Extensions;

public static class FixtureExtensions
{
    public static string CreateString(this IFixture fixture, int length, string startsWith = "")
    {
        if (length <= 0)
            return string.Empty;
        
        var stringBuilder = new StringBuilder();

        if (!string.IsNullOrEmpty(startsWith))
        {
            stringBuilder.Append(startsWith);
        }
        
        var chars = fixture.CreateMany<char>(length).ToArray();
        
        stringBuilder.Append(chars);
        
        return stringBuilder.ToString();
    }

    public static User CreateUser(this IFixture fixture)
    {
        var guid = Guid.NewGuid();
        var name = fixture.CreateString(5,"name_");
        var auth0Id = fixture.CreateString(10, "auth0_");
        
        return new User
        {
            Id = guid,
            Name = name,
            Auth0Id = auth0Id,
        };
    }
    
    public static UserCreateDto CreateUserCreateDto(this IFixture fixture)
    {
        var name = fixture.CreateString(5, "name_");
        var auth0Id = fixture.CreateString(10, "auth0_");
        
        return new UserCreateDto
        {
            Name = name,
            Auth0Id = auth0Id
        };
    }
}
