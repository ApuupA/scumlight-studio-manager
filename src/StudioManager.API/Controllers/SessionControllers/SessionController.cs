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
    }
}
