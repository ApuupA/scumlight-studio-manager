using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudioManager.API.Infrastructure;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// Band gear link: the studio creates a link per project, the band enters its gear on a public page (no login).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BandGearLinkController : ControllerBase
    {
        private readonly IBandGearLinkService _service;

        public BandGearLinkController(IBandGearLinkService service)
        {
            _service = service;
        }

        // ---------- For the studio (logged in) ----------

        /// <summary>
        /// The link of a project (token null = no link).
        /// </summary>
        [HttpGet("project/{projectId:int}")]
        public async Task<ActionResult<BandGearLinkDto>> GetLink(int projectId)
        {
            return Ok(await _service.GetLink(projectId));
        }

        /// <summary>
        /// Creates a new link - an old one stops working.
        /// </summary>
        [HttpPost("project/{projectId:int}")]
        public async Task<ActionResult<BandGearLinkDto>> CreateLink(int projectId)
        {
            return Ok(await _service.CreateLink(projectId));
        }

        /// <summary>
        /// Switches the link off.
        /// </summary>
        [HttpDelete("project/{projectId:int}")]
        public async Task<IActionResult> DeleteLink(int projectId)
        {
            await _service.DeleteLink(projectId);
            return NoContent();
        }

        // ---------- For the band: no login, the secret in the link is the key ----------

        /// <summary>
        /// Project and gear for the public page.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimits.Public)]
        [HttpGet("{token}")]
        public async Task<ActionResult<BandGearFormDto>> GetForm(string token)
        {
            var form = await _service.GetForm(token);
            return form == null ? NotFound() : Ok(form);
        }

        /// <summary>
        /// Adds a piece of gear as suggestion for the studio.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimits.Public)]
        [HttpPost("{token}/gear")]
        public async Task<ActionResult<BandGearFormDto>> AddSuggestion(string token, [FromBody] BandGearSuggestionDto dto)
        {
            return Ok(await _service.AddSuggestion(token, dto));
        }

        /// <summary>
        /// Removes a suggestion that was not accepted yet.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimits.Public)]
        [HttpDelete("{token}/gear/{id:int}")]
        public async Task<ActionResult<BandGearFormDto>> DeleteSuggestion(string token, int id)
        {
            return Ok(await _service.DeleteSuggestion(token, id));
        }
    }
}
