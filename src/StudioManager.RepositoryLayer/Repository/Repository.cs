using Microsoft.EntityFrameworkCore;
using StudioManager.DataLayer.Context;
using StudioManager.DataLayer.Models;
using StudioManager.RepositoryLayer.IRepository;
using System.Linq.Expressions;

namespace StudioManager.RepositoryLayer.Repository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetAsync(int id)
        {
            return await _dbSet.SingleOrDefaultAsync(e => e.Id == id);
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            return await WithIncludes(includes).FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            return await WithIncludes(includes).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            return await WithIncludes(includes).Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            // An entity loaded through this context is already tracked - EF detects the changed properties itself.
            // Update() is only needed for detached entities; on a tracked one it would also mark every loaded
            // related entity (e.g. the bands of a project) as modified.
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _dbSet.Update(entity);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Builds a query that also loads the given related objects (SQL JOIN), e.g. g => g.GearCategory
        /// </summary>
        private IQueryable<T> WithIncludes(Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }
    }
}
