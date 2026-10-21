using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// Mix versions of a song (v1, v2, ...) with the client's feedback.
    /// </summary>
    public class MixVersionController : BaseController<MixVersionReadDto, MixVersionCreateDto, MixVersion, IMixVersionService>
    {
        private readonly IMixVersionService _service;

        public MixVersionController(IMixVersionService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// The versions of a song, newest first, with their feedback.
        /// </summary>
        [HttpGet("song/{songId:int}")]
        public async Task<ActionResult<IEnumerable<MixVersionReadDto>>> GetForSong(int songId)
        {
            return Ok(await _service.GetForSong(songId));
        }
    }
}
