using System.Linq.Expressions;
using Domain.Entities;
using Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Filters;
using Shared.PagedList;

namespace Infrastructure;

public class TopicRepository(TopicServiceContext context) : GenericRepository<Topic>(context), ITopicRepository
{
    
}
