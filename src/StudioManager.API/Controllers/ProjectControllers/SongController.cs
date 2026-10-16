using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    public class SongController : BaseController<SongReadDto, SongCreateDto, Song, ISongService>
    {
        private readonly ISongService _service;

        public SongController(ISongService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// All entries of one project.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<SongReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }

        /// <summary>
        /// Sets the same status for several songs at once.
        /// </summary>
        [HttpPut("status")]
        public async Task<ActionResult<IEnumerable<SongReadDto>>> SetStatus([FromBody] SongStatusUpdateDto dto)
        {
            return Ok(await _service.SetStatus(dto));
        }
    }
}
