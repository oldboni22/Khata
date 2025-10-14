using Microsoft.AspNetCore.SignalR;
using NotificationService.API.Extensions;
using NotificationService.API.Middleware;
using NotificationService.API.Utilities;
using NotificationService.Infrastructure.Extensions;
using NotificationService.Infrastructure.Socket;

namespace NotificationService.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.ConfigureSerilog();
        
        builder.Services.AddAuthenticationBearer(builder.Configuration);
        builder.Services.AddAuthorization();

        builder.Services.AddSignalR();
        
        builder.Services.AddInfrastructureDependencies(builder.Configuration);
        builder.Services.AddApplicationDependencies(builder.Configuration);
        
        builder.Services.AddOpenApi();

        builder.Services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();
        
        
        builder.Services.AddControllers();
        
        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHub<NotificationHub>("/Notifications");
        app.MapControllers();
        
        app.Run();
    }
}
