using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// The deliverables checklist of a project.
    /// </summary>
    public class ProjectDeliverableController : BaseController<ProjectDeliverableReadDto, ProjectDeliverableCreateDto, ProjectDeliverable, IProjectDeliverableService>
    {
        private readonly IProjectDeliverableService _service;

        public ProjectDeliverableController(IProjectDeliverableService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// The checklist of a project.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<ProjectDeliverableReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }

        /// <summary>
        /// Adds the usual deliverables (WAV, MP3, instrumentals, DDP, vinyl master) that are not in the list yet.
        /// </summary>
        [HttpPost("project/{projectId:int}/defaults")]
        public async Task<ActionResult<IEnumerable<ProjectDeliverableReadDto>>> AddDefaults(int projectId)
        {
            return Ok(await _service.AddDefaults(projectId));
        }
    }
}
