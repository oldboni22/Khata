using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationService.DAL.Models;

namespace NotificationService.DAL.MangoService;

public interface IGenericRepository<T> where T : NotificationBase
{
    Task<T> CreateAsync(T notification);

    Task<T?> FindById(Guid id);

    Task<List<T>> FindAll(Guid userId);

    Task Delete(Guid id);

    Task<T> UpdateAsync(T notification);
}

public abstract class GenericRepository<T> : IGenericRepository<T>
    where T : NotificationBase
{
    private readonly MongoClient _client;

    private readonly IMongoCollection<T> _collection;

    public GenericRepository(IOptions<MangoServiceOptions> options)
    {
        _client = new(options.Value.ConnectionString);

        var db = _client.GetDatabase(options.Value.DatabaseName);

        _collection = db.GetCollection<T>(options.Value.CollectionName);
    }

    public async Task<T> CreateAsync(T notification)
    {
        await _collection.InsertOneAsync(notification);

        return await _collection.Find(notif => notif.Id == notification.Id).FirstAsync();
    }

    public async Task<T?> FindById(Guid id)
    {
        return await _collection.Find(notification => notification.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<T>> FindAll(Guid userId)
    {
        return await _collection.Find(notification => notification.UserId == userId).ToListAsync();
    }

    public async Task Delete(Guid id)
    {
        await _collection.DeleteOneAsync(notification => notification.Id == id);
    }

    public async Task<T> UpdateAsync(T notification)
    {
        await _collection.ReplaceOneAsync(notif => notif.Id == notification.Id, notification);

        return await _collection.Find(notif => notif.Id == notification.Id).FirstAsync();
    }
}
