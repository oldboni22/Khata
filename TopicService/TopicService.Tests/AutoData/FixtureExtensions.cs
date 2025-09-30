using AutoFixture;

namespace TopicService.Tests.AutoData;

public static class FixtureExtensions
{
    public static string CreateString(this IFixture fixture, int length)
    {
        if (length <= 0)
            return string.Empty;

        var chars = fixture.CreateMany<char>(length).ToArray();
        
        return new string(chars);
    }
}
