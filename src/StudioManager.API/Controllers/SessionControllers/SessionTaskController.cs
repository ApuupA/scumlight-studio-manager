using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.ServiceLayer.IService.ISessionServices;

namespace StudioManager.API.Controllers.SessionControllers
{
    public class SessionTaskController : BaseController<SessionTaskReadDto, SessionTaskCreateDto, SessionTask, ISessionTaskService>
    {
        private readonly ISessionTaskService _service;

        public SessionTaskController(ISessionTaskService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// All entries of one session.
        /// </summary>
        [HttpGet("session/{sessionId:int}")]
        public async Task<ActionResult<IEnumerable<SessionTaskReadDto>>> GetForSession(int sessionId)
        {
            return Ok(await _service.GetForSession(sessionId));
        }
    }
}
