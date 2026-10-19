using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Auth;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.ServiceLayer.IService.IUserServices;
using System.Security.Claims;

namespace StudioManager.API.Controllers.UserControllers
{
    /// <summary>
    /// Logins of the studio (settings page). Only owners may manage them.
    /// Every login sees the same data - projects, sessions etc. do not belong to a user.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Owner")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// All logins, sorted by name.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAll()
        {
            return Ok(await _userService.GetAll());
        }

        /// <summary>
        /// Creates another login with a start password.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> Create([FromBody] UserCreateDto dto)
        {
            return Ok(await _userService.Create(dto));
        }

        /// <summary>
        /// Sets a new password for a user, e.g. when it was forgotten.
        /// </summary>
        [HttpPut("{id:int}/password")]
        public async Task<IActionResult> SetPassword(int id, [FromBody] SetPasswordDto dto)
        {
            await _userService.SetPassword(id, dto);
            return NoContent();
        }

        /// <summary>
        /// Deletes a login. Your own login can not be deleted.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.Delete(id, CurrentUserId);
            return NoContent();
        }

        /// <summary>
        /// Id of the logged in user, stored in the login cookie at login.
        /// </summary>
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
