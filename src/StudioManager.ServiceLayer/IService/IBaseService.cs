namespace StudioManager.ServiceLayer.IService
{
    /// <summary>
    /// Generic CRUD operations for an entity T. The outside world only sees DTOs, never the entity itself.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TReadDto">DTO returned to the client.</typeparam>
    /// <typeparam name="TCreateDto">DTO received from the client for create and update.</typeparam>
    public interface IBaseService<T, TReadDto, TCreateDto> where T : class
    {
        Task<IEnumerable<TReadDto>> GetAll();

        /// <summary>
        /// Gets one entity as DTO, null if it does not exist.
        /// </summary>
        Task<TReadDto?> Get(int id);

        Task<TReadDto> Insert(TCreateDto createDto);

        /// <summary>
        /// Overwrites the entity with the values of the DTO. Throws EntityNotFoundException if it does not exist.
        /// </summary>
        Task<TReadDto> Update(int id, TCreateDto updateDto);

        /// <summary>
        /// Deletes the entity. Throws EntityNotFoundException if it does not exist.
        /// </summary>
        Task Delete(int id);
    }
}
