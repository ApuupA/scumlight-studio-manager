using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.ServiceLayer.IService.ISessionServices;

namespace StudioManager.API.Controllers.SessionControllers
{
    public class SessionController : BaseController<SessionReadDto, SessionCreateDto, Session, ISessionService>
    {
        private readonly ISessionService _service;

        public SessionController(ISessionService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// All entries of one project.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<SessionReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }

        /// <summary>
        /// Changes only the status of the session.
        /// </summary>
        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<SessionReadDto>> SetStatus(int id, [FromBody] SessionStatusUpdateDto dto)
        {
            return Ok(await _service.SetStatus(id, dto));
        }

        /// <summary>
        /// Copies the session (with songs, to-dos and gear) to the given dates, e.g. for a series of recording days.
        /// </summary>
        [HttpPost("{id:int}/duplicate")]
        public async Task<ActionResult<IEnumerable<SessionReadDto>>> Duplicate(int id, [FromBody] SessionDuplicateDto dto)
        {
            return Ok(await _service.Duplicate(id, dto));
        }
    }
}
