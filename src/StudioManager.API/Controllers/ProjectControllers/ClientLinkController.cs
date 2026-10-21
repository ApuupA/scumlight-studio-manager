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
    /// Client page (/client/{token}, no login): the client listens to the mix versions, comments with their name,
    /// approves mixes and adds references. The link itself is created with the band gear link endpoints (same secret).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClientLinkController : ControllerBase
    {
        private readonly IClientLinkService _service;

        public ClientLinkController(IClientLinkService service)
        {
            _service = service;
        }

        // ---------- For the studio (logged in) ----------

        /// <summary>
        /// Comments, approvals and references from clients that were not seen yet, per project (start page).
        /// </summary>
        [HttpGet("news")]
        public async Task<ActionResult<IEnumerable<ClientNewsDto>>> GetNews()
        {
            return Ok(await _service.GetNews());
        }

        /// <summary>
        /// Everything the client wrote in this project counts as seen (called when the tab "Mixing" is opened).
        /// </summary>
        [HttpPost("project/{projectId:int}/read")]
        public async Task<IActionResult> MarkRead(int projectId)
        {
            await _service.MarkRead(projectId);
            return NoContent();
        }

        // ---------- For the client: no login, the secret in the link is the key ----------

        /// <summary>
        /// The client page: mix versions with feedback, references.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimits.Public)]
        [HttpGet("{token}")]
        public async Task<ActionResult<ClientPageDto>> GetPage(string token)
        {
            var page = await _service.GetPage(token);
            return page == null ? NotFound() : Ok(page);
        }

        /// <summary>
        /// The client comments on a mix version.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimits.Public)]
        [HttpPost("{token}/feedback")]
        public async Task<ActionResult<ClientPageDto>> AddFeedback(string token, [FromBody] ClientFeedbackCreateDto dto)
        {
            return Ok(await _service.AddFeedback(token, dto));
        }

        /// <summary>
        /// The client approves a mix version.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimits.Public)]
        [HttpPost("{token}/versions/{mixVersionId:int}/approve")]
        public async Task<ActionResult<ClientPageDto>> ApproveVersion(string token, int mixVersionId, [FromBody] ClientApproveDto dto)
        {
            return Ok(await _service.ApproveVersion(token, mixVersionId, dto));
        }

        /// <summary>
        /// The client adds a reference track.
        /// </summary>
        [AllowAnonymous]
        [EnableRateLimiting(RateLimits.Public)]
        [HttpPost("{token}/references")]
        public async Task<ActionResult<ClientPageDto>> AddReference(string token, [FromBody] ClientReferenceCreateDto dto)
        {
            return Ok(await _service.AddReference(token, dto));
        }
    }
}
