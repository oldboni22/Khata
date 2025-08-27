using UserService.API.Extensions;
using UserService.API.Middleware;   

namespace UserService.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.ConfigureSerilog();
        
        builder.Services.AddAuthorization();
        
        builder.Services.AddAuthenticationBearer(builder.Configuration);
        
        builder.Services.AddCorsPolicies(builder.Configuration);
        
        builder.Services.AddOpenApi();

        builder.Services.AddApiDependencies(builder.Configuration);
        
        var app = builder.Build();
        
        app.UseMiddleware<ExceptionMiddleware>();
        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}
