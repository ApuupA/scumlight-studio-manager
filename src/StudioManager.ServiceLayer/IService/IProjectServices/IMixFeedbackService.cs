using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// The feedback points of a mix version.
    /// </summary>
    public interface IMixFeedbackService : IBaseService<MixFeedback, MixFeedbackReadDto, MixFeedbackCreateDto>
    {
    }
}
