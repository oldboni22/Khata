using NotificationService.API.Extensions;
using NotificationService.API.Middleware;
using NotificationService.Infrastructure.Extensions;

namespace NotificationService.API;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.ConfigureSerilog();
        
        builder.Services.AddAuthenticationBearer(builder.Configuration);
        builder.Services.AddAuthorization();
        
        builder.Services.AddInfrastructureDependencies(builder.Configuration);
        builder.Services.AddApplicationDependencies(builder.Configuration);
        
        builder.Services.AddOpenApi();

        builder.Services.AddControllers();
        
        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseMiddleware<ExceptionMiddleware>();
        
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        
        app.Run();
    }
}
