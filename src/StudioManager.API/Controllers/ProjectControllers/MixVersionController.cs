using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// Mix versions of a project ("v1" with all songs, "v1.1" with some) with the client's feedback.
    /// </summary>
    public class MixVersionController : BaseController<MixVersionReadDto, MixVersionCreateDto, MixVersion, IMixVersionService>
    {
        private readonly IMixVersionService _service;

        public MixVersionController(IMixVersionService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// The versions of a project, newest first, with their songs and feedback.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<MixVersionReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }

        /// <summary>
        /// The versions a song was exported in, newest first (song page).
        /// </summary>
        [HttpGet("song/{songId:int}")]
        public async Task<ActionResult<IEnumerable<MixVersionReadDto>>> GetForSong(int songId)
        {
            return Ok(await _service.GetForSong(songId));
        }
    }
}
