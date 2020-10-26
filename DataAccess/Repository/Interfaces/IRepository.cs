using DataAccess.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface IRepository<TEntity, TKey>
    {
        Task<IReadOnlyCollection<TEntity>> GetAllAsync();
        Task<IReadOnlyCollection<TEntity>> FilterAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task CreateAsync(TEntity entity);
        Task<TEntity> FindAsync(TKey id);
        Task DeleteAsync(TKey id);
        Task UpdateAsync(TEntity entity);

        Task<IReadOnlyCollection<TEntity>> GetWithIncludeAsync(params Expression<Func<TEntity, object>>[] includeProperties);
        Task<IReadOnlyCollection<TEntity>> FilterWithIncludeAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeProperties);
    }
}
