using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudioManager.API.Infrastructure;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.ServiceLayer.IService.IFinanceServices;

namespace StudioManager.API.Controllers.FinanceControllers
{
    /// <summary>
    /// Extra items of a project (tape, rental, external mastering, ...).
    /// </summary>
    [Authorize(Roles = Roles.Owner)] // finance: only the owner
    public class ProjectCostItemController : BaseController<ProjectCostItemReadDto, ProjectCostItemCreateDto, ProjectCostItem, IProjectCostItemService>
    {
        private readonly IProjectCostItemService _service;

        public ProjectCostItemController(IProjectCostItemService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// The items of a project, newest first.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<ProjectCostItemReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }
    }
}
