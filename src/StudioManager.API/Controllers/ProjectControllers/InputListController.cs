using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// Input lists of a project. Every response contains one row per studio input.
    /// </summary>
    public class InputListController : BaseController<InputListReadDto, InputListCreateDto, InputList, IInputListService>
    {
        private readonly IInputListService _service;

        public InputListController(IInputListService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// All input lists of one project.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<InputListReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }

        /// <summary>
        /// Saves one row (what is plugged into a studio input). An empty row frees the input.
        /// </summary>
        [HttpPut("{id:int}/rows/{studioInputId:int}")]
        public async Task<ActionResult<InputListReadDto>> SaveRow(int id, int studioInputId, [FromBody] InputListRowSaveDto dto)
        {
            return Ok(await _service.SaveRow(id, studioInputId, dto));
        }

        /// <summary>
        /// Puts the microphones / DIs of the given instruments into the free inputs.
        /// </summary>
        [HttpPost("{id:int}/fill-from-setups")]
        public async Task<ActionResult<InputListFillResultDto>> FillFromSetups(int id, [FromBody] InputListFillDto dto)
        {
            return Ok(await _service.FillFromSetups(id, dto));
        }

        /// <summary>
        /// Moves source, microphone and stand to another studio input (drag and drop in the list).
        /// </summary>
        [HttpPost("{id:int}/move")]
        public async Task<ActionResult<InputListReadDto>> MoveEntry(int id, [FromBody] InputListMoveDto dto)
        {
            return Ok(await _service.MoveEntry(id, dto));
        }

        /// <summary>
        /// Removes all rows of the list.
        /// </summary>
        [HttpDelete("{id:int}/rows")]
        public async Task<ActionResult<InputListReadDto>> ClearRows(int id)
        {
            return Ok(await _service.ClearRows(id));
        }
    }
}
