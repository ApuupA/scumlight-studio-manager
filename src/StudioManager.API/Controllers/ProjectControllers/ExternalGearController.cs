using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    public class ExternalGearController : BaseController<ExternalGearReadDto, ExternalGearCreateDto, ExternalGear, IExternalGearService>
    {
        private readonly IExternalGearService _service;

        public ExternalGearController(IExternalGearService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// All entries of one project.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<ExternalGearReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }

        /// <summary>
        /// Accepts a suggestion the band entered via the band gear link.
        /// </summary>
        [HttpPost("{id:int}/accept")]
        public async Task<ActionResult<ExternalGearReadDto>> Accept(int id)
        {
            return Ok(await _service.Accept(id));
        }
    }
}
