using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudioManager.API.Infrastructure;
using StudioManager.DataLayer.DTO.Auth;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.ServiceLayer.IService.IUserServices;
using System.Security.Claims;

namespace StudioManager.API.Controllers.AuthControllers
{
    /// <summary>
    /// Login / logout with an authentication cookie. The browser sends the cookie automatically with every request.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Logs in. Limited to 10 attempts per 15 minutes per IP address (then 429 Too Many Requests).
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimits.Login)]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<UserReadDto>> Login([FromBody] LoginDto dto)
        {
            var user = await _userService.ValidateCredentials(dto.Email, dto.Password);
            if (user == null)
            {
                return Unauthorized();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.DisplayName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true });

            return Ok(user);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return NoContent();
        }

        /// <summary>
        /// Returns the logged in user. The frontend calls this on startup to find out whether it is logged in (200) or not (401).
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserReadDto>> Me()
        {
            var user = await _userService.Get(CurrentUserId);
            return user == null ? Unauthorized() : Ok(user);
        }

        /// <summary>
        /// Changes name and email of the logged in user.
        /// </summary>
        [HttpPut("me")]
        public async Task<ActionResult<UserReadDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            return Ok(await _userService.UpdateProfile(CurrentUserId, dto));
        }

        /// <summary>
        /// Changes the password of the logged in user (requires the current password).
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            await _userService.ChangePassword(CurrentUserId, dto);
            return NoContent();
        }

        /// <summary>
        /// Id of the logged in user, stored in the login cookie at login.
        /// </summary>
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
