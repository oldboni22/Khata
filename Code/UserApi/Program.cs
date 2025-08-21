using UserApi.Extensions;

namespace UserApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.ConfigureSerilog();
        
        builder.Services.AddAuthorization();
        
        var app = builder.Build();
        app.AddExceptionMiddleware();
        
        
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}