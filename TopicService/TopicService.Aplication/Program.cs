using TopicService.API.Extensions;

namespace TopicService.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        app.Run();
    }
}
