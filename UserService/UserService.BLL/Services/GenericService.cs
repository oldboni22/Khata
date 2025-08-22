using System.Linq.Expressions;
using AutoMapper;
using UserService.BLL.Exceptions;
using UserService.BLL.Exceptions.User;
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

    public async Task<IList<TModel>> FindByConditionAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default)
    {
        var entities = await repository.FindByConditionAsync(expression,false, cancellationToken);
        
        var models = mapper.Map<IList<TModel>>(entities);

        return models;
    }

    public async Task<TModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.FindByIdAsync(id, false, cancellationToken);

        if (entity is null)
        {
            logger.Warning($"An entity of type {typeof(TEntity).Name} with id {id} was not found.");
            
            throw new UserNotFoundException(id);
        }
        
        var model = mapper.Map<TModel>(entity);

        return model;
    }

    public async Task<TModel> CreateAsync(TCreateModel createModel, CancellationToken cancellationToken = default)
    {
        var model = mapper.Map<TModel>(createModel);
        
        var entity = mapper.Map<TEntity>(model);
        
        var created = await repository.CreateAsync(entity, cancellationToken);
        
        return mapper.Map<TModel>(created);
    }

    public async Task<TModel?> UpdateAsync(Guid id, TUpdateModel updateModel, CancellationToken cancellationToken = default)
    {
        if (!await repository.ExistsAsync(id, cancellationToken))
        {
            logger.Warning($"No entity of type {typeof(TEntity).Name} with id {id} exists.");

            throw new UserNotFoundException(id);
        }
        
        var model = mapper.Map<TModel>(updateModel);
        model.Id = id;
        
        var entity = mapper.Map<TEntity>(model);
        
        var updatedEntity = await repository.UpdateAsync(entity, cancellationToken);

        return mapper.Map<TModel>(updatedEntity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await repository.ExistsAsync(id, cancellationToken))
        {
            logger.Warning($"No entity of type {typeof(TEntity).Name} with id {id} exists.");
            
            throw new UserNotFoundException(id);
        }
        
        return await repository.DeleteAsync(id, cancellationToken);
    }
}