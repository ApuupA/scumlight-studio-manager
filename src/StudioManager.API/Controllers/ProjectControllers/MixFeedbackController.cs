using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.ServiceLayer.IService.IProjectServices;

namespace StudioManager.API.Controllers.ProjectControllers
{
    /// <summary>
    /// Feedback points of a mix version (create, edit, tick off, delete).
    /// </summary>
    public class MixFeedbackController : BaseController<MixFeedbackReadDto, MixFeedbackCreateDto, MixFeedback, IMixFeedbackService>
    {
        private readonly IMixFeedbackService _service;

        public MixFeedbackController(IMixFeedbackService service) : base(service)
        {
            _service = service;
        }

        /// <summary>
        /// Creates a comment - the author is the logged in user.
        /// </summary>
        public override async Task<ActionResult<MixFeedbackReadDto>> Post([FromBody] MixFeedbackCreateDto dto)
        {
            return Ok(await _service.InsertFromStudio(dto, CurrentUserId));
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
