using FactoryX.Domain.Common;
using System.Linq.Expressions;

namespace FactoryX.Infrastructure.Contracts;

public interface IBaseRepository<TEntity> where TEntity : EntityBase
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, bool trackChanges = false);
    IQueryable<TEntity> GetAllQueryableAsync(bool trackChanges = false);
    Task<TEntity?> GetByIdAsync(int id, bool trackChanges = false);
    void Create(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}