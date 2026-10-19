using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// The arrangement of a song (song page): parts like "Chorus 1" with bars and notes.
    /// </summary>
    public class SongSectionController : BaseController<SongSectionReadDto, SongSectionCreateDto, SongSection, ISongSectionService>
    {
        private readonly ISongSectionService _service;

        public SongSectionController(ISongSectionService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// The parts of one song in song order.
        /// </summary>
        [HttpGet("song/{songId:int}")]
        public async Task<ActionResult<IEnumerable<SongSectionReadDto>>> GetForSong(int songId)
        {
            return Ok(await _service.GetForSong(songId));
        }

        /// <summary>
        /// Moves a part one position up (-1) or down (+1).
        /// </summary>
        [HttpPost("{id:int}/move")]
        public async Task<IActionResult> Move(int id, [FromBody] MoveDto dto)
        {
            await _service.Move(id, dto);
            return NoContent();
        }
    }
}
