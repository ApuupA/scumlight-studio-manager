using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.ServiceLayer.IService.ISessionServices;

namespace StudioManager.API.Controllers.SessionControllers
{
    /// <summary>
    /// Studio gear booked for sessions. Every response contains the conflict warnings of the booking.
    /// </summary>
    public class GearBookingController : BaseController<GearBookingReadDto, GearBookingCreateDto, GearBooking, IGearBookingService>
    {
        private readonly IGearBookingService _service;

        public GearBookingController(IGearBookingService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// All gear booked for one session.
        /// </summary>
        [HttpGet("session/{sessionId:int}")]
        public async Task<ActionResult<IEnumerable<GearBookingReadDto>>> GetForSession(int sessionId)
        {
            return Ok(await _service.GetForSession(sessionId));
        }

        /// <summary>
        /// All gear booked in any session of a project.
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<IEnumerable<GearBookingReadDto>>> GetForProject(int projectId)
        {
            return Ok(await _service.GetForProject(projectId));
        }

        /// <summary>
        /// Upcoming bookings of one gear item (from today on, without cancelled sessions).
        /// </summary>
        [HttpGet("gearItem/{gearItemId:int}")]
        public async Task<ActionResult<IEnumerable<GearBookingReadDto>>> GetUpcomingForGearItem(int gearItemId)
        {
            return Ok(await _service.GetUpcomingForGearItem(gearItemId));
        }
    }
}
