using System.Linq.Expressions;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.RepositoryContracts;
using Infrastructure.gRpc;
using Shared.Filters;
using TopicService.API.Utilities.LogMessages;

namespace TopicService.API.ApplicationServices;

public abstract class ApplicationServiceBase<TEntity, TSortOptions>(
    ITopicRepository repository, IUserGRpcClient userGRpcClient, IMapper mapper, Serilog.ILogger logger) 
    where TEntity : EntityBase
    where TSortOptions : Enum
{
    protected ITopicRepository Repository { get; } = repository;
    
    protected IUserGRpcClient UserGRpcClient { get; } = userGRpcClient;
    
    protected IMapper Mapper { get; } = mapper;
    
    protected Serilog.ILogger Logger { get; } = logger.ForContext<ApplicationServiceBase<TEntity, TSortOptions>>();

    protected abstract Expression<Func<TEntity, object>> ParseSortOptions(TSortOptions sortOptions);
    
    protected abstract (Expression<Func<TEntity, object>> selector, bool ascending) DefaultSortOptions { get; }
    
    protected (Expression<Func<TEntity, object>>, bool)[] ParseFilters(List<FilterEntry<TSortOptions>> entries)
    {
        (Expression<Func<TEntity, object>>, bool)[] selectors;
        
        if (entries.Count == 0)
        {
            selectors = [ DefaultSortOptions ];
        }
        else
        {
            selectors = new (Expression<Func<TEntity, object>>, bool)[entries.Count];
            
            for (int i = 0; i < entries.Count; i++)
            {
                selectors[i] = (ParseSortOptions(entries[i].SortOptions), entries[i].Ascending);
            }
        }

        return selectors;
    }
    
    protected void ThrowNotFoundException(Guid id)
    {
        logger.Information(EntityNotFoundLogMessage<TEntity>.Generate(id));
        
        throw new EntityNotFoundException<TEntity>(id);
    }  
}
