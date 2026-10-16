using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SettingsModels;
using StudioManager.ServiceLayer.IService.ISettingsServices;

namespace StudioManager.API.Controllers.SettingsControllers
{
    public class StudioInputController : BaseController<StudioInputReadDto, StudioInputCreateDto, StudioInput, IStudioInputService>
    {
        private readonly IStudioInputService _service;

        public StudioInputController(IStudioInputService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// Creates several numbered inputs at once, e.g. 8 inputs "16CH - 9" ... "16CH - 16".
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult<IEnumerable<StudioInputReadDto>>> BulkCreate([FromBody] StudioInputBulkCreateDto dto)
        {
            return Ok(await _service.BulkCreate(dto));
        }
    }
}
