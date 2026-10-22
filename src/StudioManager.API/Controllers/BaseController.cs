using Microsoft.AspNetCore.Mvc;
using StudioManager.API.Infrastructure;
using StudioManager.ServiceLayer.IService;

namespace StudioManager.API.Controllers
{
    /// <summary>
    /// Base controller providing the common CRUD endpoints. Derived controllers add their own special endpoints.
    /// Not found / business rule errors are thrown by the services and turned into 404 / 409 by ApiExceptionHandler.
    /// </summary>
    /// <typeparam name="TReadDto">DTO returned to the client.</typeparam>
    /// <typeparam name="TCreateDto">DTO received for create and update.</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TService">The service handling the entity.</typeparam>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseController<TReadDto, TCreateDto, TEntity, TService> : ControllerBase
        where TEntity : class
        where TService : IBaseService<TEntity, TReadDto, TCreateDto>
    {
        private readonly TService _service;

        protected BaseController(TService service)
        {
            _service = service;
        }

        /// <summary>
        /// True for the studio owner - assistants see no money (e.g. purchase prices of the gear).
        /// </summary>
        protected bool IsOwner => User.IsInRole(Roles.Owner);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<TReadDto>>> GetAll()
        {
            return Ok(await _service.GetAll());
        }

        /// <summary>
        /// Gets a single entity by its id.
        /// </summary>
        [HttpGet("{id:int}")]
        public virtual async Task<ActionResult<TReadDto>> Get(int id)
        {
            var dto = await _service.Get(id);
            return dto == null ? NotFound() : Ok(dto);
        }

        /// <summary>
        /// Creates a new entity.
        /// </summary>
        [HttpPost]
        public virtual async Task<ActionResult<TReadDto>> Post([FromBody] TCreateDto dto)
        {
            var created = await _service.Insert(dto);
            return Ok(created);
        }

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        [HttpPut("{id:int}")]
        public virtual async Task<ActionResult<TReadDto>> Put(int id, [FromBody] TCreateDto dto)
        {
            var updated = await _service.Update(id, dto);
            return Ok(updated);
        }

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        [HttpDelete("{id:int}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }
    }
}
