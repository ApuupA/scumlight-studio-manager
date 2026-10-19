using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// The recording checklist of a song (song page): tracks and their progress.
    /// </summary>
    public class SongTrackController : BaseController<SongTrackReadDto, SongTrackCreateDto, SongTrack, ISongTrackService>
    {
        private readonly ISongTrackService _service;

        public SongTrackController(ISongTrackService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// The tracks of one song in checklist order.
        /// </summary>
        [HttpGet("song/{songId:int}")]
        public async Task<ActionResult<IEnumerable<SongTrackReadDto>>> GetForSong(int songId)
        {
            return Ok(await _service.GetForSong(songId));
        }

        /// <summary>
        /// Moves a track one position up (-1) or down (+1).
        /// </summary>
        [HttpPost("{id:int}/move")]
        public async Task<IActionResult> Move(int id, [FromBody] MoveDto dto)
        {
            await _service.Move(id, dto);
            return NoContent();
        }

        /// <summary>
        /// Adds one track per instrument of the project (names already on the checklist are skipped).
        /// </summary>
        [HttpPost("song/{songId:int}/from-instruments")]
        public async Task<ActionResult<IEnumerable<SongTrackReadDto>>> AddFromInstruments(int songId)
        {
            return Ok(await _service.AddFromInstruments(songId));
        }

        /// <summary>
        /// Copies the track names of another song of the same project.
        /// </summary>
        [HttpPost("copy")]
        public async Task<ActionResult<IEnumerable<SongTrackReadDto>>> CopyFromSong([FromBody] SongTrackCopyDto dto)
        {
            return Ok(await _service.CopyFromSong(dto));
        }
    }
}
