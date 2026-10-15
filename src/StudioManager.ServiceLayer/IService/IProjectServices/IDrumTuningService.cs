using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    public interface IDrumTuningService : IBaseService<DrumTuning, DrumTuningReadDto, DrumTuningCreateDto>
    {
        /// <summary>
        /// All DrumTuning entries of one project.
        /// </summary>
        Task<IEnumerable<DrumTuningReadDto>> GetForProject(int projectId);
    }
}
