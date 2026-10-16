using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// Recorded instruments of a project, each returned with its complete setup.
    /// </summary>
    public class ProjectInstrumentController : BaseController<ProjectInstrumentReadDto, ProjectInstrumentCreateDto, ProjectInstrument, IProjectInstrumentService>
    {
        private readonly IProjectInstrumentService _service;

        public ProjectInstrumentController(IProjectInstrumentService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// All instruments of one project with their setup.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<ProjectInstrumentReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }

        /// <summary>
        /// Creates a new instrument from a setup template.
        /// </summary>
        [HttpPost("from-template")]
        public async Task<ActionResult<ProjectInstrumentReadDto>> FromTemplate([FromBody] ApplySetupTemplateDto dto)
        {
            return Ok(await _service.ApplyTemplate(dto));
        }
    }
}
