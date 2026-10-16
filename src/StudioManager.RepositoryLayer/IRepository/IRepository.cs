using System.Linq.Expressions;

namespace StudioManager.RepositoryLayer.IRepository
{
    /// <summary>
    /// Generic data access for one entity type. Every write method saves its changes immediately.
    /// </summary>
    public interface IRepository<T>
    {
        /// <summary>
        /// Gets an entity by its id, null if it does not exist.
        /// </summary>
        Task<T?> GetAsync(int id);

        /// <summary>
        /// Gets the first entity matching the predicate and includes related objects, null if none matches.
        /// E.g. GetAsync(g => g.Id == 5, g => g.GearCategory) loads the gear item together with its category.
        /// </summary>
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Gets all entities and includes related objects.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Gets all entities matching the predicate and includes related objects.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

        /// <summary>
        /// Same as above, but the related objects are given as text paths. Needed to load objects "through" a list,
        /// e.g. "SetupItems.GearItem.GearCategory" loads the setup items of an instrument with their gear and its category.
        /// </summary>
        Task<IEnumerable<T>> GetAllWithPathsAsync(Expression<Func<T, bool>> predicate, params string[] includePaths);

        /// <summary>
        /// Returns true if at least one entity matches the predicate.
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        /// <summary>
        /// Saves changes of several entities in one go (e.g. a new status for several songs).
        /// </summary>
        Task UpdateRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Deletes the entity with the given id. Does nothing if it does not exist.
        /// </summary>
        Task DeleteAsync(int id);
    }
}
