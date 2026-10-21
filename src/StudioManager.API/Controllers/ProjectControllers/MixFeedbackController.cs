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
        public MixFeedbackController(IMixFeedbackService service) : base(service)
        {
        }
    }
}
