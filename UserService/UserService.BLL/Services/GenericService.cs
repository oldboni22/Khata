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
    Task<PagedList<TModel>> FindByConditionAsync(
        Expression<Func<TEntity, bool>> expression, PaginationParameters paginationParameters, CancellationToken cancellationToken = default);
    
    Task<TModel?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<TModel> CreateAsync(TCreateModel createModel, CancellationToken cancellationToken = default);
    
    Task<TModel?> UpdateAsync(Guid id, TUpdateModel updateModel, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GenericService<TEntity, TModel, TCreateModel, TUpdateModel>(
    IGenericRepository<TEntity> repository, 
    IMapper mapper, 
    Serilog.ILogger logger) :
    IGenericService<TEntity, TModel, TCreateModel, TUpdateModel>
    where TEntity : EntityBase
    where TModel : ModelBase
    where TCreateModel : class
    where TUpdateModel : class
{
    protected IMapper Mapper { get; } = mapper;

    protected Serilog.ILogger Logger { get; } = logger;
    
    public async Task<PagedList<TModel>> FindByConditionAsync(
        Expression<Func<TEntity, bool>> expression, PaginationParameters paginationParameters, CancellationToken cancellationToken = default)
    {
        var entities = await repository
            .FindByConditionAsync(expression, paginationParameters,false, cancellationToken);
        
        var models = Mapper.Map<PagedList<TModel>>(entities);

        return models;
    }

    public async Task<TModel?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await repository
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
        var entity = Mapper.Map<TEntity>(createModel);
        
        var created = await repository.CreateAsync(entity, cancellationToken);
        
        return Mapper.Map<TModel>(created);
    }

    public async Task<TModel?> UpdateAsync(Guid id, TUpdateModel updateModel, CancellationToken cancellationToken = default)
    {
        var target = await FindByIdAsync(id, cancellationToken);
        
        if (target is null)
        {
            Logger.Warning(EntityNotFoundLogMessageGenerator<TEntity>.GenerateMessage(id));

            throw new EntityNotFoundException<TEntity>(id);
        }
        
        var entity = Mapper.Map<TEntity>(updateModel);
        entity.Id = id;
        
        var updatedEntity = await repository.UpdateAsync(entity, cancellationToken);

        return Mapper.Map<TModel>(updatedEntity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var target = await FindByIdAsync(id, cancellationToken);
        
        if (target is null)
        {
            Logger.Warning(EntityNotFoundLogMessageGenerator<TEntity>.GenerateMessage(id));
            
            throw new EntityNotFoundException<TEntity>(id);
        }
        
        await repository.DeleteAsync(id, cancellationToken);
    }
}
