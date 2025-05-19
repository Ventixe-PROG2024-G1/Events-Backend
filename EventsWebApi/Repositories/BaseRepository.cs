using EventsWebApi.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EventsWebApi.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<bool> AddAsync(TEntity entity);
        Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> UpdateAsync(TEntity entity);
        IQueryable<TEntity> GetQueryable();
    }

    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<TEntity> _table;

        protected BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _table = _context.Set<TEntity>();
        }

        public virtual async Task<bool> AddAsync(TEntity entity)
        {
            if (entity == null)
                return false;

            try
            {
                await _table.AddAsync(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException) // Fånga specifikt databasuppdateringsfel
            {
                return false;
            }
            catch (Exception) // Fånga andra oväntade fel
            {
                return false;
            }
        }

        public virtual async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
                return false;

            try
            {
                var entity = await _table.FirstOrDefaultAsync(predicate);
                if (entity == null)
                    return false;

                _table.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual IQueryable<TEntity> GetQueryable()
        {
            try
            {
                return _table.AsQueryable();
            }
            catch (Exception)
            {
                return Enumerable.Empty<TEntity>().AsQueryable();
            }
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            try
            {
                return await _table.ToListAsync();
            }
            catch (Exception)
            {
                return Enumerable.Empty<TEntity>();
            }
        }

        public virtual async Task<TEntity?> GetByIdAsync(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
                return null;

            try
            {
                return await _table.FirstOrDefaultAsync(predicate);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<bool> UpdateAsync(TEntity entity)
        {
            if (entity == null)
                return false;

            try
            {
                _table.Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
