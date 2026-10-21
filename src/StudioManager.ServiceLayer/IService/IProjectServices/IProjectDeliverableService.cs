using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    /// <summary>
    /// The deliverables checklist of a project (WAV, MP3, DDP, vinyl master, ...).
    /// </summary>
    public interface IProjectDeliverableService : IBaseService<ProjectDeliverable, ProjectDeliverableReadDto, ProjectDeliverableCreateDto>
    {
        /// <summary>The checklist of a project in its order.</summary>
        Task<IEnumerable<ProjectDeliverableReadDto>> GetForProject(int projectId);

        /// <summary>Adds the usual deliverables that are not in the list yet.</summary>
        Task<IEnumerable<ProjectDeliverableReadDto>> AddDefaults(int projectId);
    }
}
