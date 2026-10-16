using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// Entries of an instrument setup (studio gear, band gear or free text).
    /// </summary>
    public class InstrumentSetupItemController : BaseController<InstrumentSetupItemReadDto, InstrumentSetupItemCreateDto, InstrumentSetupItem, IInstrumentSetupItemService>
    {
        private readonly IInstrumentSetupItemService _service;

        public InstrumentSetupItemController(IInstrumentSetupItemService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// Moves the entry one position up (-1) or down (+1) in its chain / list.
        /// </summary>
        [HttpPost("{id:int}/move")]
        public async Task<IActionResult> Move(int id, [FromBody] SetupItemMoveDto dto)
        {
            await _service.Move(id, dto);
            return NoContent();
        }

        /// <summary>
        /// In which projects and for which instruments a studio gear item is used.
        /// </summary>
        [HttpGet("gearItem/{gearItemId:int}/usages")]
        public async Task<ActionResult<IEnumerable<GearUsageDto>>> GetUsagesOfGearItem(int gearItemId)
        {
            return Ok(await _service.GetUsagesOfGearItem(gearItemId));
        }
    }
}
