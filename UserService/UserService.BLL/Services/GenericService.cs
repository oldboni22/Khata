using System.Linq.Expressions;
using AutoMapper;
using UserService.BLL.Exceptions;
using UserService.BLL.Models;
using UserService.DAL.Models.Entities;
using UserService.DAL.Repositories;

namespace UserService.BLL.Services;


public interface IGenericService<TEntity, TModel, in TCreateModel, in TUpdateModel>
    where TEntity : EntityBase
    where TModel : ModelBase
    where TCreateModel : class
    where TUpdateModel : class
{
    Task<IList<TModel>> FindByConditionAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
    
    Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<TModel> CreateAsync(TCreateModel createModel, CancellationToken cancellationToken = default);
    
    Task<TModel?> UpdateAsync(Guid id, TUpdateModel updateModel, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GenericService<TEntity, TModel, TCreateModel, TUpdateModel>(IGenericRepository<TEntity> repository, IMapper mapper, Serilog.ILogger logger) :
    IGenericService<TEntity, TModel, TCreateModel, TUpdateModel>
    where TEntity : EntityBase
    where TModel : ModelBase
    where TCreateModel : class
    where TUpdateModel : class
{
    protected readonly IGenericRepository<TEntity> Repository = repository;
    protected readonly IMapper Mapper = mapper;
    protected readonly Serilog.ILogger Logger = logger;

    public async Task<IList<TModel>> FindByConditionAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
    {
        var entities = await Repository.FindByConditionAsync(expression,false, cancellationToken);
        
        var models = Mapper.Map<IList<TModel>>(entities);

        return models;
    }

    public async Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Repository.FindByIdAsync(id, false, cancellationToken);

        if (entity == null)
        {
            Logger.Warning($"An entity of type {typeof(TEntity).Name} with id {id} was not found.");
            
            throw new UserNotFoundException(id);
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
        if (! await Repository.ExistsAsync(id, cancellationToken))
        {
            Logger.Warning($"No entity of type {typeof(TEntity).Name} with id {id} exists.");

            throw new UserNotFoundException(id);
        }
        
        var model = Mapper.Map<TModel>(updateModel);
        model.Id = id;
        
        var entity = Mapper.Map<TEntity>(model);
        
        var updatedEntity = await Repository.UpdateAsync(entity, cancellationToken);

        return Mapper.Map<TModel>(updatedEntity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (! await Repository.ExistsAsync(id, cancellationToken))
        {
            Logger.Warning($"No entity of type {typeof(TEntity).Name} with id {id} exists.");
            
            throw new UserNotFoundException(id);
        }
        
        return await Repository.DeleteAsync(id, cancellationToken);
    }
}