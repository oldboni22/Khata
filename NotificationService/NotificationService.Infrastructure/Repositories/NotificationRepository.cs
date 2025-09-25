using Messages.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotificationService.DAL.MangoService;
using NotificationService.Domain.Contracts.Repos;
using NotificationService.Infrastructure.MangoService;
using Shared;
using Shared.PagedList;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository(IOptions<MangoServiceOptions> options, TimeProvider timeProvider)
    : GenericRepository<Notification>(options, timeProvider), INotificationRepository
{
    public async Task<PagedList<Notification>> FindAllNotificationsAsync(
        Guid userId, PaginationParameters paginationParameters, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Notification>.Filter.Eq(notification => notification.UserId, userId);
        var sort = Builders<Notification>.Sort.Descending(notification => notification.CreatedAt);
        
        return await GeneralizedFind(filter, sort, paginationParameters, cancellationToken);
    }

    public async Task<PagedList<Notification>> FindUnreadNotificationsAsync(
        Guid userId, PaginationParameters paginationParameters, CancellationToken cancellationToken = default)
    {
        var idFilter = Builders<Notification>.Filter.Eq(notification => notification.UserId, userId);
        var stateFilter = Builders<Notification>.Filter.Eq(notification => notification.ReadAt, null);
        
        var filter = Builders<Notification>.Filter.And(idFilter, stateFilter);
        
        var sort = Builders<Notification>.Sort.Descending(notification => notification.CreatedAt);
        
        return await GeneralizedFind(filter, sort, paginationParameters, cancellationToken);
    }

    public async Task<IEnumerable<Notification>> FindUnreadNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var idFilter = Builders<Notification>.Filter.Eq(notification => notification.UserId, userId);
        var stateFilter = Builders<Notification>.Filter.Eq(notification => notification.ReadAt, null);
        
        var filter = Builders<Notification>.Filter.And(idFilter, stateFilter);
        
        return await Collection.Find(filter).ToListAsync(cancellationToken: cancellationToken);
    }

    private async Task<PagedList<Notification>> GeneralizedFind(
        FilterDefinition<Notification> filter, 
        SortDefinition<Notification> sort, 
        PaginationParameters paginationParameters,
        CancellationToken cancellationToken = default)
    {
        var totalCount = (int)await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        
        var pageCount = (int)Math.Ceiling(totalCount / (double)paginationParameters.PageSize);
        
        var notifications = await Collection
            .Find(filter)
            .Sort(sort)
            .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
            .Limit(paginationParameters.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);
        
        return notifications
            .ToPagedList(paginationParameters.PageNumber, paginationParameters.PageSize, pageCount, totalCount);
    }
}