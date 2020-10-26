using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public class BaseRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
    {
        protected TimeOffTrackerContext _context;

        private DbSet<TEntity> _entities;
        protected DbSet<TEntity> Entities => this._entities ??= _context.Set<TEntity>();
        protected BaseRepository(TimeOffTrackerContext context)
        {
            _context = context;
        }

        public virtual async Task CreateAsync(TEntity entity)
        {
            await Entities.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TKey id)
        {
            Entities.Remove(await Entities.FindAsync(id));
            await _context.SaveChangesAsync();
        }

        public virtual async Task<IReadOnlyCollection<TEntity>> FilterAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Entities.Where(predicate).ToListAsync();
        }

        public virtual async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Entities.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<TEntity> FindAsync(TKey id)
        {
            return await Entities.FindAsync(id);
        }

        public virtual async Task<IReadOnlyCollection<TEntity>> GetAllAsync()
        {
            return await Entities.ToListAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<TEntity>> GetWithIncludeAsync(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            return await Include(includeProperties).ToListAsync();
        }

        public async Task<IReadOnlyCollection<TEntity>> FilterWithIncludeAsync(Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var query = Include(includeProperties);
            return await query.Where(predicate).ToListAsync();
        }

        private IQueryable<TEntity> Include(params Expression<Func<TEntity, object>>[] includeProperties)
        {
            IQueryable<TEntity> query = Entities.AsNoTracking();
            return includeProperties
                .Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }
    }
}
