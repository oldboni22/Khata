using Messages.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationService.DAL.MangoService;
using NotificationService.Domain.Contracts.Repos;

namespace NotificationService.Infrastructure.MangoService;

public class GenericRepository<T> : IGenericRepository<T>
    where T : Notification
{
    protected IMongoCollection<T> Collection { get; }
    
    public GenericRepository(IOptions<MangoServiceOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);

        var db = client.GetDatabase(options.Value.DatabaseName);

        Collection = db.GetCollection<T>(options.Value.CollectionName);
    }

    public async Task CreateManyAsync(IEnumerable<T> notifications)
    {
        await Collection.InsertManyAsync(notifications);
    }

    public async Task<T?> FindById(Guid id)
    {
        return await Collection.Find(notification => notification.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<T>> FindAll(Guid userId)
    {
        return await Collection.Find(notification => notification.UserId == userId).ToListAsync();
    }

    public async Task<bool> Delete(Guid id)
    {
        var result = await Collection.DeleteOneAsync(notification => notification.Id == id);

        return result.DeletedCount > 0;
    }

    public async Task UpdateAsync(T notification)
    {
        await Collection.ReplaceOneAsync(notif => notif.Id == notification.Id, notification);
    }

    public async Task UpdateManyAsync(IEnumerable<T> notifications)
    {
        var writeModels = notifications
            .Select(notification =>
            {
                var filter = Builders<T>.Filter.Eq(n => n.Id, notification.Id);
                
                return new ReplaceOneModel<T>(filter, notification);
            });

        await Collection.BulkWriteAsync(writeModels, new BulkWriteOptions
        {
            IsOrdered = false
        });
    }
}
