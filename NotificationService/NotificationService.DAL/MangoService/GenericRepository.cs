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

public abstract class GenericRepository<T>(IOptions<MangoServiceOptions> options) : IGenericRepository<T> 
    where T : NotificationBase
{
    private readonly MongoClient _client = new(options.Value.ConnectionString);

    private readonly string _dbName = options.Value.DatabaseName;
    
    private readonly string _collectionName = options.Value.CollectionName;

    public async Task<T> CreateAsync(T notification)
    {
        var db = _client.GetDatabase(_dbName);
        var collection = db.GetCollection<T>(_collectionName);
        
        await collection.InsertOneAsync(notification);
        
        return await collection.Find(notif => notif.Id == notification.Id).FirstAsync();
    }
        
    public async Task<T?> FindById(Guid id)
    {
        var db  = _client.GetDatabase(_dbName);
        var collection = db.GetCollection<T>(_collectionName);
        
        return await collection.Find(notification => notification.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<T>> FindAll(Guid userId)
    {
        var db  = _client.GetDatabase(_dbName);
        var collection = db.GetCollection<T>(_collectionName);
        
        return await collection.Find(notification => notification.UserId == userId).ToListAsync();
    }
    
    public async Task Delete(Guid id)
    {
        var db = _client.GetDatabase(_dbName);
        var collection = db.GetCollection<T>(_collectionName);
        
        await collection.DeleteOneAsync(notification => notification.Id == id);
    }
    
    public async Task<T> UpdateAsync(T notification)
    {
        var db = _client.GetDatabase(_dbName);
        var collection = db.GetCollection<T>(_collectionName);
        
        await collection.ReplaceOneAsync(notif => notif.Id == notification.Id, notification);
        
        return await collection.Find(notif => notif.Id == notification.Id).FirstAsync();
    }
}