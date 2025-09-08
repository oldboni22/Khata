using System.Linq.Expressions;
using AutoMapper;
using Domain.Contracts.GRpc;
using Domain.Contracts.RepositoryContracts;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.Filters;

namespace TopicService.API.Controllers;

public abstract class BaseController<TEntity, TSortOptions>(
    ITopicRepository repository, IUserGRpcClient userGRpcClient, IMapper mapper, Serilog.ILogger logger) : ControllerBase
    where TEntity : EntityBase
    where TSortOptions : Enum
{
    protected ITopicRepository Repository { get; } = repository;
    
    protected IUserGRpcClient UserGRpcClient { get; } = userGRpcClient;
    
    protected IMapper Mapper { get; } = mapper;
    
    protected Serilog.ILogger Logger { get; } = logger.ForContext<BaseController<TEntity, TSortOptions>>();

    protected abstract Expression<Func<TEntity, object>> ParseSortOption(TSortOptions sortOption);
    
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
                selectors[i] = (ParseSortOption(entries[i].SortOptions), entries[i].Ascending);
            }
        }

        return selectors;
    }
}
