using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using MinIoService;
using NSubstitute;
using NSubstitute.ClearExtensions;
using Shared;
using UserService.API;
using UserService.BLL.gRpc;
using UserService.DAL.CacheService;
using UserService.IntegrationTests.Extensions;

namespace UserService.IntegrationTests;

public class UserServiceTestFactory : WebApplicationFactory<Program>
{
    public const string Auth0ApiKey = "гойда";
    
    public IMinioService MinioServiceMock { get; init; } = Substitute.For<IMinioService>();

    public ITopicGRpcClient TopicGRpcClientMock { get; init; } = Substitute.For<ITopicGRpcClient>();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection
            ([
                new KeyValuePair<string, string>(ConfigurationKeys.Auth0ApiKey, Auth0ApiKey)!,
                new KeyValuePair<string, string>(ConfigurationKeys.SerilogFile, "log.txt")!,
            ]);
        });
        
        builder.ConfigureTestServices(services =>
        {
            services
                .GenericReplace<IMinioService>(MinioServiceMock)
                .GenericReplace<ITopicGRpcClient>(TopicGRpcClientMock)
                .ReplaceDependencies();
        });
    }

    public void ClearMocks()
    {
        MinioServiceMock.ClearReceivedCalls();
        MinioServiceMock.ClearSubstitute();
        
        TopicGRpcClientMock.ClearReceivedCalls();
        TopicGRpcClientMock.ClearSubstitute();
    }
}
