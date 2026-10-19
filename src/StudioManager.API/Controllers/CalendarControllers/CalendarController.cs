using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.ServiceLayer.IService.ICalendarServices;
using System.Security.Claims;

namespace StudioManager.API.Controllers.CalendarControllers
{
    /// <summary>
    /// Calendar subscription for Nextcloud, phones, Outlook, ...: the sessions as .ics feed behind a secret link.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        /// <summary>
        /// The secret of the logged in user's subscription link (null if not created yet).
        /// </summary>
        [HttpGet("token")]
        public async Task<ActionResult<CalendarTokenDto>> GetToken()
        {
            return Ok(await _calendarService.GetToken(CurrentUserId));
        }

        /// <summary>
        /// Creates a new subscription link - the old one stops working.
        /// </summary>
        [HttpPost("token")]
        public async Task<ActionResult<CalendarTokenDto>> CreateToken()
        {
            return Ok(await _calendarService.CreateToken(CurrentUserId));
        }

        /// <summary>
        /// The sessions as iCalendar feed. No login (calendar apps can not log in) - the secret in the link is the key.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{token}.ics")]
        [Produces("text/calendar")]
        public async Task<IActionResult> GetFeed(string token)
        {
            var feed = await _calendarService.GetFeed(token);
            return feed == null ? NotFound() : Content(feed, "text/calendar; charset=utf-8");
        }

        /// <summary>
        /// Id of the logged in user, stored in the login cookie at login.
        /// </summary>
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
