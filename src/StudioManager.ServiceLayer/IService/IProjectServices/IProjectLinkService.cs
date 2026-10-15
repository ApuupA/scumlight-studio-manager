using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.ServiceLayer.IService.IProjectServices
{
    public interface IProjectLinkService : IBaseService<ProjectLink, ProjectLinkReadDto, ProjectLinkCreateDto>
    {
        /// <summary>
        /// All ProjectLink entries of one project.
        /// </summary>
        Task<IEnumerable<ProjectLinkReadDto>> GetForProject(int projectId);
    }
}
