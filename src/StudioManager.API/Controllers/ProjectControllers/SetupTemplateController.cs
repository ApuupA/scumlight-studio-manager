using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// Reusable instrument setups. Not based on BaseController: templates are only created from an instrument
    /// and applied via ProjectInstrumentController/from-template - there is no normal create / update.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SetupTemplateController : ControllerBase
    {
        private readonly ISetupTemplateService _service;

        public SetupTemplateController(ISetupTemplateService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SetupTemplateReadDto>>> GetAll()
        {
            return Ok(await _service.GetAll());
        }

        /// <summary>
        /// Saves the setup of an instrument as template (without band gear).
        /// </summary>
        [HttpPost("from-instrument")]
        public async Task<ActionResult<SetupTemplateReadDto>> FromInstrument([FromBody] SetupTemplateFromInstrumentDto dto)
        {
            return Ok(await _service.CreateFromInstrument(dto));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.Delete(id);
            return NoContent();
        }
    }
}
