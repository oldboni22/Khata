using Microsoft.EntityFrameworkCore;
using UserService.DAL;

namespace UserService.API.Extensions;

public static class WebApplicationExtensions
{
    public static void UpdateUserDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        var userServiceContext = scope.ServiceProvider.GetRequiredService<UserServiceContext>();
        userServiceContext.Database.Migrate();
    }
}
