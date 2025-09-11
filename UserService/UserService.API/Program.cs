using Microsoft.AspNetCore.Server.Kestrel.Core;
using Shared;
using UserService.API.Extensions;
using UserService.API.Middleware;
using UserService.BLL.gRpc;

namespace UserService.API;

public class Program
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
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddApiDependencies(builder.Configuration);
        
        var app = builder.Build();
        
        app.UseMiddleware<ExceptionMiddleware>();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseHttpsRedirection();

        app.MapGrpcService<UserGRpcApi>();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        
        app.Run();
    }
}
