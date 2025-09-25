using NotificationService.API.Extensions;
using NotificationService.Infrastructure.Extensions;

namespace NotificationService.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddAuthorization();

        builder.Services.AddInfrastructureDependencies(builder.Configuration);
        builder.Services.AddApplicationDependencies(builder.Configuration);
        
        builder.Services.AddOpenApi();

        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.Run();
    }
}
