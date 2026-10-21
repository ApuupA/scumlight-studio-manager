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
        /// Saves the lyrics of a song (song page).
        /// </summary>
        [HttpPut("{id:int}/lyrics")]
        public async Task<ActionResult<SongReadDto>> SetLyrics(int id, [FromBody] SongLyricsDto dto)
        {
            return Ok(await _service.SetLyrics(id, dto));
        }

        /// <summary>
        /// Saves the mixing notes of a song: problems in the material and the mix recall (song page).
        /// </summary>
        [HttpPut("{id:int}/mix-info")]
        public async Task<ActionResult<SongReadDto>> SetMixInfo(int id, [FromBody] SongMixInfoDto dto)
        {
            return Ok(await _service.SetMixInfo(id, dto));
        }

        /// <summary>
        /// Saves the master values of a song: ISRC, LUFS, true peak, pause before the song (tab "Mastering").
        /// </summary>
        [HttpPut("{id:int}/master-info")]
        public async Task<ActionResult<SongReadDto>> SetMasterInfo(int id, [FromBody] SongMasterInfoDto dto)
        {
            return Ok(await _service.SetMasterInfo(id, dto));
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
