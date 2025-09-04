using System.Linq.Expressions;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.RepositoryContracts;
using Infrastructure.gRpc;
using TopicService.API.Utilities.LogMessages;

namespace TopicService.API.ApplicationServices;

public abstract class ApplicationServiceBase<TEntity, TSortOptions>(
    IGenericRepository<TEntity> repository, IUserGRpcClient userGRpcClient, IMapper mapper, Serilog.ILogger logger) 
    where TEntity : EntityBase
    where TSortOptions : Enum
{
    protected IGenericRepository<TEntity> Repository { get; } = repository;
    
    protected IUserGRpcClient UserGRpcClient { get; } = userGRpcClient;
    
    protected IMapper Mapper { get; } = mapper;
    
    protected Serilog.ILogger Logger { get; } = logger.ForContext<ApplicationServiceBase<TEntity, TSortOptions>>();

    protected abstract Expression<Func<TEntity, object>> ParseSortOptions(TSortOptions sortOptions);
    
    protected abstract (Expression<Func<TEntity, object>> selector, bool ascending) DefaultSortOptions { get; }
    
    protected (Expression<Func<TEntity, object>>, bool)[] ParseFilters((TSortOptions selector, bool ascending)[] filters)
    {
        (Expression<Func<TEntity, object>>, bool)[] selectors;
        
        if (filters.Length == 0)
        {
            selectors = [ DefaultSortOptions ];
        }
        else
        {
            selectors = new (Expression<Func<TEntity, object>>, bool)[filters.Length];
            
            for (int i = 0; i < filters.Length; i++)
            {
                selectors[i] = (ParseSortOptions(filters[i].selector), filters[i].ascending);
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
