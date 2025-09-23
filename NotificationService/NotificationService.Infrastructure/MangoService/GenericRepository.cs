using Messages.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationService.DAL.MangoService;
using NotificationService.Domain.Contracts.Repos;

namespace NotificationService.Infrastructure.MangoService;

public class GenericRepository<T> : IGenericRepository<T>
    where T : Notification
{
    private readonly MongoClient _client;

    private readonly IMongoCollection<T> _collection;

    public GenericRepository(IOptions<MangoServiceOptions> options)
    {
        _client = new(options.Value.ConnectionString);

        var db = _client.GetDatabase(options.Value.DatabaseName);

        _collection = db.GetCollection<T>(options.Value.CollectionName);
    }

    public async Task CreateManyAsync(IEnumerable<T> notifications)
    {
        var createdAt = DateTime.UtcNow;
        notifications = notifications.Select(notif =>
        {
            notif.CreatedAt = createdAt;
            return notif;
        });
        
        await _collection.InsertManyAsync(notifications);
    }

    public async Task<T?> FindById(Guid id)
    {
        return await _collection.Find(notification => notification.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<T>> FindAll(Guid userId)
    {
        return await _collection.Find(notification => notification.UserId == userId).ToListAsync();
    }

    public async Task<bool> Delete(Guid id)
    {
        var result = await _collection.DeleteOneAsync(notification => notification.Id == id);

        return result.DeletedCount > 0;
    }

    public async Task<T?> UpdateAsync(T notification)
    {
        var result = await _collection.ReplaceOneAsync(notif => notif.Id == notification.Id, notification);

        if (result.ModifiedCount == 0)
        {
            return null;
        }
        
        return await _collection.Find(notif => notif.Id == notification.Id).FirstAsync();
    }
}
