using AutoFixture;

namespace UserService.IntegrationTests.Utils;

public class AutoFixtureContainer
{
    public IFixture Fixture { get; init; } = new Fixture();
}
