using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.ServiceLayer.IService.IFinanceServices;
using System.Security.Claims;

namespace StudioManager.API.Controllers.FinanceControllers
{
    /// <summary>
    /// Working time on projects, including the timer of the logged in user.
    /// </summary>
    public class TimeEntryController : BaseController<TimeEntryReadDto, TimeEntryCreateDto, TimeEntry, ITimeEntryService>
    {
        private readonly ITimeEntryService _service;

        public TimeEntryController(ITimeEntryService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// The entries of a project, newest first.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<TimeEntryReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }

        /// <summary>
        /// The running timer of the logged in user (204 No Content if none is running).
        /// </summary>
        [HttpGet("timer")]
        public async Task<ActionResult<TimeEntryReadDto>> GetTimer()
        {
            var timer = await _service.GetRunningTimer(CurrentUserId);
            return timer == null ? NoContent() : Ok(timer);
        }

        /// <summary>
        /// Starts the timer of the logged in user.
        /// </summary>
        [HttpPost("timer/start")]
        public async Task<ActionResult<TimeEntryReadDto>> StartTimer([FromBody] TimerStartDto dto)
        {
            return Ok(await _service.StartTimer(CurrentUserId, dto));
        }

        /// <summary>
        /// Stops the timer: the time is rounded up to the quarter hour and saved as entry.
        /// </summary>
        [HttpPost("timer/stop")]
        public async Task<ActionResult<TimeEntryReadDto>> StopTimer([FromBody] TimerStopDto dto)
        {
            return Ok(await _service.StopTimer(CurrentUserId, dto));
        }

        /// <summary>
        /// Id of the logged in user, stored in the login cookie at login.
        /// </summary>
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
