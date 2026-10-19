using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    public interface IExternalGearService : IBaseService<ExternalGear, ExternalGearReadDto, ExternalGearCreateDto>
    {
        /// <summary>
        /// All ExternalGear entries of one project.
        /// </summary>
        Task<IEnumerable<ExternalGearReadDto>> GetForProject(int projectId);

        /// <summary>
        /// Accepts a suggestion of the band: it becomes normal band gear (usable in setups, printed).
        /// </summary>
        Task<ExternalGearReadDto> Accept(int id);
    }
}
