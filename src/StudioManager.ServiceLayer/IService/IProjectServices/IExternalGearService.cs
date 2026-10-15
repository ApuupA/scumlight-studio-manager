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
    }
}
