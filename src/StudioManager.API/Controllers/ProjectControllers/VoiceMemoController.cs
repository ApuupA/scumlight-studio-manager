using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;
using StudioManager.ServiceLayer.Service.ProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// Voice memos for songwriting: upload, play and sort them to songs and parts.
    /// </summary>
    public class VoiceMemoController : BaseController<VoiceMemoReadDto, VoiceMemoCreateDto, VoiceMemo, IVoiceMemoService>
    {
        private readonly IVoiceMemoService _service;

        public VoiceMemoController(IVoiceMemoService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// The memos of a project, newest recording first.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<VoiceMemoReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }

        /// <summary>
        /// Uploads one audio file (M4A, MP3, WAV, ..., max. 20 MB) as a new memo.
        /// recordedOn: the date of the file on the phone; songId: optional, the memo belongs to this song right away.
        /// </summary>
        [HttpPost("project/{projectId:int}/upload")]
        [RequestSizeLimit(VoiceMemoService.MaxMemoSizeBytes + 1024 * 1024)]
        public async Task<ActionResult<VoiceMemoReadDto>> Upload(int projectId, IFormFile file, [FromQuery] DateOnly? recordedOn, [FromQuery] int? songId)
        {
            await using var stream = file.OpenReadStream();
            return Ok(await _service.Upload(projectId, stream, file.FileName, file.Length, recordedOn, songId));
        }

        /// <summary>
        /// The audio file for the player. Range requests are supported, so the player can jump to any position.
        /// </summary>
        [HttpGet("{id:int}/file")]
        public async Task<IActionResult> GetFile(int id)
        {
            var file = await _service.GetFile(id);
            if (file == null)
            {
                return NotFound();
            }

            var disposition = new ContentDispositionHeaderValue("inline");
            disposition.SetHttpFileName(file.FileName ?? "memo");
            Response.Headers.ContentDisposition = disposition.ToString();
            return File(file.Content, file.ContentType, enableRangeProcessing: true);
        }
    }
}
