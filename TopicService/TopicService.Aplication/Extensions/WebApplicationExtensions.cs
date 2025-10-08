using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace TopicService.API.Extensions;

public static class WebApplicationExtensions
{
    public static void UpdateTopicDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        var topicServiceContext = scope.ServiceProvider.GetRequiredService<TopicServiceContext>();
        
        if(topicServiceContext.Database.IsRelational())
        {
            topicServiceContext.Database.Migrate();
        }
    }
}
