using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;
using StudioManager.ServiceLayer.Service.FileServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    public class ProjectController : BaseController<ProjectReadDto, ProjectCreateDto, Project, IProjectService>
    {
        private readonly IProjectService _service;

        public ProjectController(IProjectService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// All entries of one client.
        /// </summary>
        [HttpGet("client/{clientId:int}")]
        public async Task<ActionResult<IEnumerable<ProjectReadDto>>> GetForClient(int clientId)
        {
            return Ok(await _service.GetForClient(clientId));
        }

        /// <summary>
        /// Sets which parts of the work belong to the project: recording, mixing, mastering (at least one).
        /// </summary>
        [HttpPut("{id:int}/scope")]
        public async Task<ActionResult<ProjectReadDto>> SetScope(int id, [FromBody] ProjectScopeDto dto)
        {
            return Ok(await _service.SetScope(id, dto));
        }

        /// <summary>
        /// The picture as image file (404 if there is none).
        /// </summary>
        [HttpGet("{id:int}/photo")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photo = await _service.GetPhoto(id);
            return photo == null ? NotFound() : File(photo.Content, photo.ContentType);
        }

        /// <summary>
        /// Uploads a picture (JPG, PNG or WEBP, max. 10 MB) and replaces the existing one.
        /// </summary>
        [HttpPost("{id:int}/photo")]
        [RequestSizeLimit(PhotoService.MaxPhotoSizeBytes + 1024 * 1024)]
        public async Task<ActionResult<ProjectReadDto>> UploadPhoto(int id, IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            return Ok(await _service.SetPhoto(id, stream, file.FileName, file.Length));
        }

        /// <summary>
        /// Removes the picture.
        /// </summary>
        [HttpDelete("{id:int}/photo")]
        public async Task<ActionResult<ProjectReadDto>> DeletePhoto(int id)
        {
            return Ok(await _service.DeletePhoto(id));
        }
    }
}
