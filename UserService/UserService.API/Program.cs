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
            var addressStr = builder.Configuration[ConfigurationKeys.UserGRpcPort];
            var port =  int.Parse(addressStr!);
            
            options.ListenAnyIP(7082, listenOptions =>
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
            //GRpc &^^
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
