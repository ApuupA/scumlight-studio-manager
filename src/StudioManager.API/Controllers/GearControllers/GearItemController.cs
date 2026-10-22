using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.ServiceLayer.IService.IGearServices;
using StudioManager.ServiceLayer.Service.FileServices;

namespace StudioManager.API.Controllers.GearControllers
{
    /// <summary>
    /// The gear of the studio. Everybody may use and edit it - but only the owner sees the purchase price (money).
    /// </summary>
    public class GearItemController : BaseController<GearItemReadDto, GearItemCreateDto, GearItem, IGearItemService>
    {
        private readonly IGearItemService _service;

        public GearItemController(IGearItemService service) : base(service)
        {
            _service = service;
        }

        public override async Task<ActionResult<IEnumerable<GearItemReadDto>>> GetAll()
        {
            return Ok(HidePrice(await _service.GetAll()));
        }

        public override async Task<ActionResult<GearItemReadDto>> Get(int id)
        {
            var item = await _service.Get(id);
            return item == null ? NotFound() : Ok(HidePrice(item));
        }

        public override async Task<ActionResult<GearItemReadDto>> Post([FromBody] GearItemCreateDto dto)
        {
            if (!IsOwner)
            {
                dto.PurchasePrice = null;
            }
            return Ok(HidePrice(await _service.Insert(dto)));
        }

        public override async Task<ActionResult<GearItemReadDto>> Put(int id, [FromBody] GearItemCreateDto dto)
        {
            // An assistant does not see the price - it would send it back empty and delete it. So the saved one is kept.
            if (!IsOwner)
            {
                dto.PurchasePrice = (await _service.Get(id))?.PurchasePrice;
            }
            return Ok(HidePrice(await _service.Update(id, dto)));
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
        [RequestSizeLimit(PhotoService.MaxPhotoSizeBytes + 1024 * 1024)]
        public async Task<ActionResult<GearItemReadDto>> UploadPhoto(int id, IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            var updated = await _service.SetPhoto(id, stream, file.FileName, file.Length);
            return Ok(HidePrice(updated));
        }

        /// <summary>
        /// Removes the photo of a gear item.
        /// </summary>
        [HttpDelete("{id:int}/photo")]
        public async Task<ActionResult<GearItemReadDto>> DeletePhoto(int id)
        {
            return Ok(HidePrice(await _service.DeletePhoto(id)));
        }

        /// <summary>The item without the purchase price for assistants.</summary>
        private GearItemReadDto HidePrice(GearItemReadDto item)
        {
            if (!IsOwner)
            {
                item.PurchasePrice = null;
            }
            return item;
        }

        private IEnumerable<GearItemReadDto> HidePrice(IEnumerable<GearItemReadDto> items)
        {
            return items.Select(HidePrice).ToList();
        }
    }
}
