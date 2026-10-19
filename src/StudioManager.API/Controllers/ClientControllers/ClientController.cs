using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.ServiceLayer.IService.IClientServices;
using StudioManager.ServiceLayer.Service.FileServices;

namespace StudioManager.API.Controllers.ClientControllers
{
    public class ClientController : BaseController<ClientReadDto, ClientCreateDto, Client, IClientService>
    {
        private readonly IClientService _service;

        public ClientController(IClientService service) : base(service)
        {
            _service = service;
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
        public async Task<ActionResult<ClientReadDto>> UploadPhoto(int id, IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            return Ok(await _service.SetPhoto(id, stream, file.FileName, file.Length));
        }

        /// <summary>
        /// Removes the picture.
        /// </summary>
        [HttpDelete("{id:int}/photo")]
        public async Task<ActionResult<ClientReadDto>> DeletePhoto(int id)
        {
            return Ok(await _service.DeletePhoto(id));
        }
    }
}
