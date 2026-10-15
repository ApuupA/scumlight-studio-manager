using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    public class ProjectController : BaseController<ProjectReadDto, ProjectCreateDto, Project, IProjectService>
    {
        private readonly IProjectService _service;

        public ProjectController(IProjectService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// All entries of one client.
        /// </summary>
        [HttpGet("client/{clientId:int}")]
        public async Task<ActionResult<IEnumerable<ProjectReadDto>>> GetForClient(int clientId)
        {
            return Ok(await _service.GetForClient(clientId));
        }
    }
}
