using Infrastructure.gRpc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Shared;
using TopicService.API.Extensions;
using TopicService.API.Middleware;

namespace TopicService.API;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.ConfigureSerilog();
        
        builder.WebHost.ConfigureKestrel(options =>
        {
            var grpcPortStr = builder.Configuration[ConfigurationKeys.TopicGRpcPort];
            var grpcPort =  int.Parse(grpcPortStr!);
            
            var appPortStr = builder.Configuration[ConfigurationKeys.ApplicationPort];
            var appPort = int.Parse(appPortStr!);
            
            options.ListenAnyIP(appPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                listenOptions.UseHttps();
            });
            //Rest APi ^
            
            options.ListenLocalhost(grpcPort, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
                listenOptions.UseHttps();
            });
            //GRpc & ^
        });
        
        builder.Services.AddAuthenticationBearer(builder.Configuration);
        builder.Services.AddAuthorization();
        
        builder.Services.AddOpenApi();

        builder.Services.AddApplicationLayerDependencies(builder.Configuration);
        
        builder.Services.AddControllers();
        
        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseMiddleware<ExceptionMiddleware>();
        
        app.UseHttpsRedirection();

        app.MapGrpcService<GRpcService>();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        app.Run();
    }
}
