using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    public interface IProjectService : IBaseService<Project, ProjectReadDto, ProjectCreateDto>
    {
        /// <summary>
        /// All Project entries of one client.
        /// </summary>
        Task<IEnumerable<ProjectReadDto>> GetForClient(int clientId);
    }
}
