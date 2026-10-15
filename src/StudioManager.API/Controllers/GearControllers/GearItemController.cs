using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.ServiceLayer.IService.IGearServices;
using StudioManager.ServiceLayer.Service.GearServices;

namespace StudioManager.API.Controllers.GearControllers
{
    public class GearItemController : BaseController<GearItemReadDto, GearItemCreateDto, GearItem, IGearItemService>
    {
        private readonly IGearItemService _service;

        public GearItemController(IGearItemService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// Returns the photo of a gear item as image file.
        /// </summary>
        [HttpGet("{id:int}/photo")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, "image/jpeg", "image/png", "image/webp")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photo = await _service.GetPhoto(id);
            return photo == null ? NotFound() : File(photo.Content, photo.ContentType);
        }

        /// <summary>
        /// Uploads a photo (JPG, PNG or WEBP, max. 10 MB) and replaces the existing one.
        /// </summary>
        [HttpPost("{id:int}/photo")]
        [RequestSizeLimit(GearItemService.MaxPhotoSizeBytes + 1024 * 1024)]
        public async Task<ActionResult<GearItemReadDto>> UploadPhoto(int id, IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var updated = await _service.SetPhoto(id, stream, file.FileName, file.Length);
            return Ok(updated);
        }

        /// <summary>
        /// Removes the photo of a gear item.
        /// </summary>
        [HttpDelete("{id:int}/photo")]
        public async Task<ActionResult<GearItemReadDto>> DeletePhoto(int id)
        {
            return Ok(await _service.DeletePhoto(id));
        }
    }
}
