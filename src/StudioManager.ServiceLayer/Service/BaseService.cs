using AutoMapper;
using StudioManager.DataLayer.Models;
using StudioManager.RepositoryLayer.IRepository;
using StudioManager.ServiceLayer.Exceptions;
using StudioManager.ServiceLayer.IService;
using System.Linq.Expressions;

namespace StudioManager.ServiceLayer.Service
{
    /// <summary>
    /// Generic CRUD implementation. Derived services override single methods to add business rules.
    /// </summary>
    public class BaseService<T, TReadDto, TCreateDto> : IBaseService<T, TReadDto, TCreateDto>
        where T : BaseEntity
        where TReadDto : class
        where TCreateDto : class
    {
        protected readonly IRepository<T> _repository;
        protected readonly IMapper _mapper;

        public BaseService(IRepository<T> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Related objects that are loaded together with the entity for Get / GetAll,
        /// e.g. the GearCategory of a GearItem so the DTO can contain the category name.
        /// </summary>
        protected virtual Expression<Func<T, object>>[] DefaultIncludes => [];

        public virtual async Task<IEnumerable<TReadDto>> GetAll()
        {
            var entities = await _repository.GetAllAsync(DefaultIncludes);
            return _mapper.Map<IEnumerable<TReadDto>>(entities);
        }

        /// <summary>
        /// All entities matching the predicate (with DefaultIncludes), e.g. all songs of one project:
        /// GetAllWhere(s => s.ProjectId == projectId)
        /// </summary>
        protected async Task<IEnumerable<TReadDto>> GetAllWhere(Expression<Func<T, bool>> predicate)
        {
            var entities = await _repository.GetAllAsync(predicate, DefaultIncludes);
            return _mapper.Map<IEnumerable<TReadDto>>(entities);
        }

        public virtual async Task<TReadDto?> Get(int id)
        {
            var entity = await _repository.GetAsync(e => e.Id == id, DefaultIncludes);
            return entity == null ? null : _mapper.Map<TReadDto>(entity);
        }

        public virtual async Task<TReadDto> Insert(TCreateDto createDto)
        {
            var entity = _mapper.Map<T>(createDto);
            await _repository.AddAsync(entity);

            // Reload so the DTO contains the included related objects as well
            return (await Get(entity.Id))!;
        }

        public virtual async Task<TReadDto> Update(int id, TCreateDto updateDto)
        {
            var entity = await _repository.GetAsync(id)
                ?? throw new EntityNotFoundException(typeof(T).Name, id);

            _mapper.Map(updateDto, entity);
            await _repository.UpdateAsync(entity);

            return (await Get(id))!;
        }

        public virtual async Task Delete(int id)
        {
            if (!await _repository.ExistsAsync(e => e.Id == id))
            {
                throw new EntityNotFoundException(typeof(T).Name, id);
            }

            await _repository.DeleteAsync(id);
        }
    }
}
