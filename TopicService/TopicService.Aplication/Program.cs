using Infrastructure.gRpc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Shared;
using TopicService.API.Extensions;

namespace TopicService.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.ConfigureSerilog();
        
        builder.WebHost.ConfigureKestrel(options =>
        {
            var addressStr = builder.Configuration[ConfigurationKeys.TopicGRpcPort];
            var port =  int.Parse(addressStr!);
            
            options.ListenAnyIP(7195, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                listenOptions.UseHttps();
            });
            //Rest APi ^
            
            options.ListenLocalhost(port, listenOptions =>
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

        app.UseHttpsRedirection();

        app.MapGrpcService<GRpcService>();
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        app.Run();
    }
}
