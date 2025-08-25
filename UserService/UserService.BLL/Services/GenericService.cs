using System.Linq.Expressions;
using AutoMapper;
using Shared.PagedList;
using UserService.BLL.Exceptions;
using UserService.BLL.Models;
using UserService.BLL.Utilities.MessageGenerators.Logs;
using UserService.DAL.Models.Entities;
using UserService.DAL.Repositories;

namespace UserService.BLL.Services;

public interface IGenericService<TEntity, TModel, in TCreateModel, in TUpdateModel>
    where TEntity : EntityBase
    where TModel : ModelBase
    where TCreateModel : class
    where TUpdateModel : class
{
    Task<PagedList<TModel>> FindByConditionAsync(Expression<Func<TEntity, bool>> expression, PagedListQueryParameters pagedParameters,
        CancellationToken cancellationToken = default);
    
    Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<TModel> CreateAsync(TCreateModel createModel, CancellationToken cancellationToken = default);
    
    Task<TModel?> UpdateAsync(Guid id, TUpdateModel updateModel, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GenericService<TEntity, TModel, TCreateModel, TUpdateModel>
    (IGenericRepository<TEntity> repository, IMapper mapper, Serilog.ILogger logger) :
    IGenericService<TEntity, TModel, TCreateModel, TUpdateModel>
    where TEntity : EntityBase
    where TModel : ModelBase
    where TCreateModel : class
    where TUpdateModel : class
{
    protected IGenericRepository<TEntity> Repository { get; } = repository;

    protected IMapper Mapper { get; } = mapper;

    protected Serilog.ILogger Logger { get; } = logger;
    
    public async Task<PagedList<TModel>> FindByConditionAsync(Expression<Func<TEntity, bool>> expression, PagedListQueryParameters pagedParameters, 
        CancellationToken cancellationToken = default)
    {
        var entities = await Repository
            .FindByConditionAsync(expression, pagedParameters,false, cancellationToken);
        
        var models = Mapper.Map<PagedList<TModel>>(entities);

        return models;
    }

    public async Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository
            .FindByIdAsync(id, false, cancellationToken);

        if (entity is null)
        {
            Logger.Warning(EntityNotFoundLogMessageGenerator<TEntity>.GenerateMessage(id));
            
            throw new EntityNotFoundException<TEntity>(id);
        }
        
        var model = Mapper.Map<TModel>(entity);

        return model;
    }

    public async Task<TModel> CreateAsync(TCreateModel createModel, CancellationToken cancellationToken = default)
    {
        var model = Mapper.Map<TModel>(createModel);
        
        var entity = Mapper.Map<TEntity>(model);
        
        var created = await Repository.CreateAsync(entity, cancellationToken);
        
        return Mapper.Map<TModel>(created);
    }

    public async Task<TModel?> UpdateAsync(Guid id, TUpdateModel updateModel, CancellationToken cancellationToken = default)
    {
        if (!await Repository.ExistsAsync(id, cancellationToken))
        {
            Logger.Warning(EntityNotFoundLogMessageGenerator<TEntity>.GenerateMessage(id));

            throw new EntityNotFoundException<TEntity>(id);
        }
        
        var model = Mapper.Map<TModel>(updateModel);
        model.Id = id;
        
        var entity = Mapper.Map<TEntity>(model);
        
        var updatedEntity = await Repository.UpdateAsync(entity, cancellationToken);

        return Mapper.Map<TModel>(updatedEntity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await Repository.ExistsAsync(id, cancellationToken))
        {
            Logger.Warning(EntityNotFoundLogMessageGenerator<TEntity>.GenerateMessage(id));
            
            throw new EntityNotFoundException<TEntity>(id);
        }
        
        await Repository.DeleteAsync(id, cancellationToken);
    }
}
